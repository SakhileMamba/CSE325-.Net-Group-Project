using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;

namespace SocialMediaApp.Services;

public sealed record LikeSummary(int Count, bool IsLikedByCurrentUser);

public class LikeService(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    AuthenticationStateProvider authenticationStateProvider)
{
    private async Task<string> GetUserIdAsync()
    {
        var user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (user.Identity?.IsAuthenticated != true || string.IsNullOrEmpty(id))
            throw new UnauthorizedAccessException("Please log in to like content.");
        return id;
    }

    public async Task<LikeSummary?> GetPostLikeAsync(int postId)
    {
        var userId = await GetUserIdAsync();
        await using var db = await contextFactory.CreateDbContextAsync();
        if (!await db.Posts.AnyAsync(p => p.Id == postId)) return null;
        return new LikeSummary(
            await db.PostLikes.CountAsync(l => l.PostId == postId),
            await db.PostLikes.AnyAsync(l => l.PostId == postId && l.UserId == userId));
    }

    public async Task<Dictionary<int, LikeSummary>> GetCommentLikesAsync(int postId)
    {
        var userId = await GetUserIdAsync();
        await using var db = await contextFactory.CreateDbContextAsync();
        var commentIds = await db.Comments.AsNoTracking().Where(c => c.PostId == postId)
            .Select(c => c.Id).ToListAsync();
        var counts = await db.CommentLikes.AsNoTracking().Where(l => commentIds.Contains(l.CommentId))
            .GroupBy(l => l.CommentId).Select(group => new { Id = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Id, item => item.Count);
        var mine = await db.CommentLikes.AsNoTracking()
            .Where(l => commentIds.Contains(l.CommentId) && l.UserId == userId)
            .Select(l => l.CommentId).ToHashSetAsync();
        return commentIds.ToDictionary(id => id,
            id => new LikeSummary(counts.GetValueOrDefault(id), mine.Contains(id)));
    }

    public async Task<LikeSummary?> TogglePostLikeAsync(int postId)
    {
        var userId = await GetUserIdAsync();
        await using var db = await contextFactory.CreateDbContextAsync();
        if (!await db.Posts.AnyAsync(p => p.Id == postId)) return null;
        var like = await db.PostLikes.FindAsync(postId, userId);
        if (like is null) db.PostLikes.Add(new PostLike { PostId = postId, UserId = userId });
        else db.PostLikes.Remove(like);
        await db.SaveChangesAsync();
        return new LikeSummary(await db.PostLikes.CountAsync(l => l.PostId == postId), like is null);
    }

    public async Task<LikeSummary?> ToggleCommentLikeAsync(int commentId)
    {
        var userId = await GetUserIdAsync();
        await using var db = await contextFactory.CreateDbContextAsync();
        if (!await db.Comments.AnyAsync(c => c.Id == commentId)) return null;
        var like = await db.CommentLikes.FindAsync(commentId, userId);
        if (like is null) db.CommentLikes.Add(new CommentLike { CommentId = commentId, UserId = userId });
        else db.CommentLikes.Remove(like);
        await db.SaveChangesAsync();
        return new LikeSummary(await db.CommentLikes.CountAsync(l => l.CommentId == commentId), like is null);
    }
}
