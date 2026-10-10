using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SocialMediaApp.Data;
using SocialMediaApp.Services;

// Run with: dotnet run --project PostManagementChecks
// Uses a temporary SQLite database, never the application's database.
var path = Path.Combine(Path.GetTempPath(), $"socialnet-post-checks-{Guid.NewGuid()}.db");
var services = new ServiceCollection();
services.Configure<IdentityOptions>(options => options.Stores.SchemaVersion = IdentitySchemaVersions.Version3);
using var provider = services.BuildServiceProvider();
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlite($"Data Source={path};Pooling=False").UseApplicationServiceProvider(provider).Options;
var factory = new CheckContextFactory(options);
var auth = new CheckAuthenticationStateProvider();
var service = new PostService(factory, auth);
var passed = 0;

void Check(bool condition, string name)
{
    if (!condition) throw new Exception($"FAILED: {name}");
    Console.WriteLine($"PASS: {name}");
    passed++;
}

async Task Reject<T>(Func<Task> action, string name) where T : Exception
{
    try { await action(); }
    catch (T) { Check(true, name); return; }
    throw new Exception($"FAILED: {name}");
}

try
{
    await using (var db = factory.CreateDbContext())
    {
        await db.Database.MigrateAsync();
        db.Users.AddRange(new ApplicationUser { Id = "owner-a", UserName = "a" },
            new ApplicationUser { Id = "owner-b", UserName = "b" });
        await db.SaveChangesAsync();
    }

    auth.SignIn("owner-a");
    await service.CreateAsync("First post");
    var post = (await service.GetMyPostsAsync()).Single();
    Check(post.Content == "First post" && post.UserId == "owner-a" &&
        post.CreatedAtUtc > DateTime.UtcNow.AddMinutes(-1), "Create and read with owner and UTC timestamp");
    var timestamp = post.CreatedAtUtc;
    Check(await service.UpdateAsync(post.Id, "Edited post"), "Update own post");
    post = (await service.GetMyPostsAsync()).Single();
    Check(post.Content == "Edited post" && post.CreatedAtUtc == timestamp, "Update persists and keeps creation time");
    await service.CreateAsync(new string('x', 250));
    Check((await service.GetMyPostsAsync()).Count == 2, "Accept 250 characters");
    await Reject<ValidationException>(() => service.CreateAsync(new string('x', 251)), "Reject create over 250 characters");
    await Reject<ValidationException>(() => service.UpdateAsync(post.Id, new string('x', 251)), "Reject edit over 250 characters");
    await Reject<ValidationException>(() => service.CreateAsync(""), "Reject empty content");
    await Reject<ValidationException>(() => service.CreateAsync(" \t\n"), "Reject whitespace-only content");
    await Reject<ValidationException>(() => service.UpdateAsync(post.Id, " "), "Reject whitespace-only edit");

    auth.SignIn("owner-b");
    Check((await service.GetMyPostsAsync()).Count == 0, "My Posts excludes other owners");
    Check(!await service.UpdateAsync(post.Id, "Unauthorized edit"), "Reject another user's update");
    Check(!await service.DeleteAsync(post.Id), "Reject another user's delete");
    await service.CreateAsync("Owner B post");
    var feed = await service.GetFeedAsync();
    Check(feed.Count == 3 && feed[0].Content == "Owner B post" && feed[0].User.UserName == "b" &&
        feed.Any(p => p.UserId == "owner-a"), "Feed shows every user's posts, newest first, with authors");
    Check((await service.GetFeedAsync(feed[0].Id, pageSize: 1)).Single().Id == feed[1].Id,
        "Feed loads the next page after the oldest post shown");

    auth.SignOut();
    await Reject<UnauthorizedAccessException>(() => service.GetMyPostsAsync(), "Reject unauthenticated read");
    await Reject<UnauthorizedAccessException>(() => service.GetFeedAsync(), "Reject unauthenticated feed read");
    await Reject<UnauthorizedAccessException>(() => service.CreateAsync("Anonymous"), "Reject unauthenticated create");
    await Reject<UnauthorizedAccessException>(() => service.UpdateAsync(post.Id, "Anonymous"), "Reject unauthenticated update");
    await Reject<UnauthorizedAccessException>(() => service.DeleteAsync(post.Id), "Reject unauthenticated delete");

    auth.SignIn("owner-a");
    Check((await service.GetMyPostsAsync()).Single(p => p.Id == post.Id).Content == "Edited post",
        "Rejected operations leave original content unchanged");
    Check(await service.DeleteAsync(post.Id), "Delete own post");
    Check(!(await service.GetMyPostsAsync()).Any(p => p.Id == post.Id), "Deletion persists");
    Check(!await service.UpdateAsync(post.Id, "Missing") && !await service.DeleteAsync(post.Id), "Handle missing posts");

    await using (var db = factory.CreateDbContext())
    {
        db.Posts.Add(new Post { Content = new string('x', 251), UserId = "owner-a", CreatedAtUtc = DateTime.UtcNow });
        await Reject<DbUpdateException>(async () => { await db.SaveChangesAsync(); }, "SQLite enforces content limit");
    }
    await using (var db = factory.CreateDbContext())
    {
        db.Posts.Add(new Post { Content = "", UserId = "owner-a", CreatedAtUtc = DateTime.UtcNow });
        await Reject<DbUpdateException>(async () => { await db.SaveChangesAsync(); }, "SQLite rejects empty content");
    }
    await using (var db = factory.CreateDbContext())
    {
        db.Posts.Add(new Post { Content = "No owner", UserId = "missing-user", CreatedAtUtc = DateTime.UtcNow });
        await Reject<DbUpdateException>(async () => { await db.SaveChangesAsync(); }, "SQLite requires an existing owner");
    }
    // Comments share the same Identity setup and migrated temporary database.
    var discussions = new CommentService(factory, auth);
    var discussionPost = (await service.GetMyPostsAsync()).Single();
    await discussions.AddAsync(discussionPost.Id, "Root comment");
    var root = (await discussions.GetCommentsAsync(discussionPost.Id)).Single();
    Check(root.ParentCommentId is null && root.UserId == "owner-a" && root.Content == "Root comment",
        "Create root comment with authenticated author");
    auth.SignIn("owner-b");
    Check((await discussions.GetPostAsync(discussionPost.Id))?.UserId == "owner-a",
        "Read another user's post for discussion");
    await discussions.AddAsync(discussionPost.Id, "Reply from B", root.Id);
    var reply = (await discussions.GetCommentsAsync(discussionPost.Id)).Single(c => c.ParentCommentId == root.Id);
    Check(reply.UserId == "owner-b" && reply.User.UserName == "b", "Reply across users with author display data");
    await discussions.AddAsync(discussionPost.Id, "Nested reply", reply.Id);
    await discussions.AddAsync(discussionPost.Id, "Second root");
    var thread = await discussions.GetCommentsAsync(discussionPost.Id);
    Check(thread.Count == 4 && thread.Count(c => c.ParentCommentId is null) == 2 &&
        thread.Any(c => c.ParentCommentId == reply.Id), "Read threaded replies and separate roots");
    var otherPost = (await service.GetMyPostsAsync()).Single();
    await Reject<ValidationException>(() => discussions.AddAsync(otherPost.Id, "Wrong post", root.Id),
        "Reject reply to a comment on another post");
    await Reject<ValidationException>(() => discussions.AddAsync(discussionPost.Id, "Missing parent", int.MaxValue),
        "Reject missing parent comment");
    await Reject<ValidationException>(() => discussions.AddAsync(int.MaxValue, "Missing post"), "Reject missing post");
    Check(await discussions.GetPostAsync(int.MaxValue) is null, "Missing discussion post returns null");
    await Reject<ValidationException>(() => discussions.AddAsync(discussionPost.Id, " \t\n"), "Reject blank comment");
    await Reject<ValidationException>(() => discussions.AddAsync(discussionPost.Id, "", root.Id), "Reject blank reply");
    Check((await discussions.GetCommentsAsync(otherPost.Id)).Count == 0, "Comments isolated by post");
    auth.SignOut();
    await Reject<UnauthorizedAccessException>(() => discussions.AddAsync(discussionPost.Id, "Anonymous"),
        "Reject unauthenticated comment");
    await Reject<UnauthorizedAccessException>(() => discussions.AddAsync(discussionPost.Id, "Anonymous", root.Id),
        "Reject unauthenticated reply");
    await Reject<UnauthorizedAccessException>(() => discussions.GetCommentsAsync(discussionPost.Id),
        "Reject unauthenticated discussion read");
    await Reject<UnauthorizedAccessException>(() => discussions.GetPostAsync(discussionPost.Id),
        "Reject unauthenticated post discussion read");
    auth.SignIn("owner-a");
    Check(await service.DeleteAsync(discussionPost.Id), "Delete post containing nested discussion");
    Check((await discussions.GetCommentsAsync(discussionPost.Id)).Count == 0,
        "Deleting post removes all comments and nested replies");

    // Account deletion removes its comments but preserves other authors' replies.
    await discussions.AddAsync(otherPost.Id, "Author A comment");
    var accountRoot = (await discussions.GetCommentsAsync(otherPost.Id)).Single();
    auth.SignIn("owner-b");
    await discussions.AddAsync(otherPost.Id, "Author B reply", accountRoot.Id);
    await using (var db = factory.CreateDbContext())
    {
        db.Users.Remove(await db.Users.SingleAsync(u => u.Id == "owner-a"));
        await db.SaveChangesAsync();
    }
    var preserved = (await discussions.GetCommentsAsync(otherPost.Id)).Single();
    Check(preserved.UserId == "owner-b" && preserved.ParentCommentId is null,
        "Account deletion preserves other authors' replies as root comments");
    Console.WriteLine($"All {passed} checks passed.");
}
finally
{
    File.Delete(path);
    File.Delete(path + "-wal");
    File.Delete(path + "-shm");
}

sealed class CheckContextFactory(DbContextOptions<ApplicationDbContext> options)
    : IDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext() => new(options);
}

sealed class CheckAuthenticationStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal user = new(new ClaimsIdentity());
    public void SignIn(string id) => user = new ClaimsPrincipal(new ClaimsIdentity(
        new[] { new Claim(ClaimTypes.NameIdentifier, id) }, "Checks"));
    public void SignOut() => user = new ClaimsPrincipal(new ClaimsIdentity());
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(user));
}
