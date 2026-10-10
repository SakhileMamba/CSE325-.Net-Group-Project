using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;

namespace SocialMediaApp.Services;

public class CommentService(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    AuthenticationStateProvider authenticationStateProvider)
{
    private async Task<string> GetUserIdAsync()
    {
        var user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (user.Identity?.IsAuthenticated != true || string.IsNullOrEmpty(id))
            throw new UnauthorizedAccessException("Please log in to participate in discussions.");
        return id;
    }

    public async Task<Post?> GetPostAsync(int postId)
    {
        await GetUserIdAsync();
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Posts.AsNoTracking().Include(p => p.User)
            .SingleOrDefaultAsync(p => p.Id == postId);
    }

    public async Task<List<Comment>> GetCommentsAsync(int postId)
    {
        await GetUserIdAsync();
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Comments.AsNoTracking().Include(c => c.User)
            .Where(c => c.PostId == postId).OrderBy(c => c.CreatedAtUtc).ThenBy(c => c.Id)
            .ToListAsync();
    }

    // Comments for a page of feed posts, loaded in one query so the feed can show
    // reply counts and open threads without going back to the database.
    public async Task<List<Comment>> GetCommentsForPostsAsync(List<int> postIds)
    {
        await GetUserIdAsync();
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Comments.AsNoTracking().Include(c => c.User)
            .Where(c => postIds.Contains(c.PostId)).OrderBy(c => c.CreatedAtUtc).ThenBy(c => c.Id)
            .ToListAsync();
    }

    public async Task AddAsync(int postId, string content, int? parentCommentId = null)
    {
        var userId = await GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(content))
            throw new ValidationException("Please enter comment content.");
        await using var db = await contextFactory.CreateDbContextAsync();
        if (!await db.Posts.AnyAsync(p => p.Id == postId))
            throw new ValidationException("This post is no longer available.");
        if (parentCommentId is int parentId && !await db.Comments.AnyAsync(
            c => c.Id == parentId && c.PostId == postId))
            throw new ValidationException("The comment you are replying to is no longer available on this post.");

        db.Comments.Add(new Comment
        {
            PostId = postId, Content = content.Trim(), UserId = userId,
            ParentCommentId = parentCommentId, CreatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}
