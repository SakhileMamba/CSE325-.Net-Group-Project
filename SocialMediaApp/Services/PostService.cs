using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;

namespace SocialMediaApp.Services;

// Identity supplies the owner; callers only supply content and a post ID.
public class PostService(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    AuthenticationStateProvider authenticationStateProvider)
{
    private async Task<string> GetUserIdAsync()
    {
        var user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (user.Identity?.IsAuthenticated != true || string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("Please log in to manage posts.");
        return userId;
    }

    private static string ValidateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length > 250)
            throw new ValidationException("Post content must contain 1 to 250 characters.");
        return content.Trim();
    }

    public async Task<List<Post>> GetMyPostsAsync()
    {
        var userId = await GetUserIdAsync();
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Posts.AsNoTracking().Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAtUtc).ThenByDescending(p => p.Id).ToListAsync();
    }

    // Every user's posts, newest first. Ids grow as posts are created, so the next
    // page is the posts with an Id lower than the oldest one already shown.
    public async Task<List<Post>> GetFeedAsync(int? beforeId = null, int pageSize = 20)
    {
        await GetUserIdAsync();
        await using var db = await contextFactory.CreateDbContextAsync();
        IQueryable<Post> query = db.Posts.AsNoTracking().Include(p => p.User);
        if (beforeId is int id) query = query.Where(p => p.Id < id);
        return await query.OrderByDescending(p => p.Id).Take(pageSize).ToListAsync();
    }

    public async Task CreateAsync(string content)
    {
        var userId = await GetUserIdAsync();
        content = ValidateContent(content);
        await using var db = await contextFactory.CreateDbContextAsync();
        db.Posts.Add(new Post { Content = content, UserId = userId, CreatedAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(int postId, string content)
    {
        var userId = await GetUserIdAsync();
        content = ValidateContent(content);
        await using var db = await contextFactory.CreateDbContextAsync();
        var post = await db.Posts.SingleOrDefaultAsync(p => p.Id == postId && p.UserId == userId);
        if (post is null) return false;
        post.Content = content;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int postId)
    {
        var userId = await GetUserIdAsync();
        await using var db = await contextFactory.CreateDbContextAsync();
        var post = await db.Posts.SingleOrDefaultAsync(p => p.Id == postId && p.UserId == userId);
        if (post is null) return false;
        db.Posts.Remove(post);
        await db.SaveChangesAsync();
        return true;
    }
}
