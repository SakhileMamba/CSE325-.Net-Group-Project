using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;

namespace SocialMediaApp.Services;

public sealed record NotificationItem(
    int Id, string Kind, int PostId, int? CommentId, string ActorUsername,
    string ActorDisplayName, string? ActorProfilePicture, DateTime CreatedAtUtc, bool IsRead);

public class NotificationService(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    AuthenticationStateProvider authenticationStateProvider)
{
    private async Task<string> GetUserIdAsync()
    {
        var user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (user.Identity?.IsAuthenticated != true || string.IsNullOrEmpty(id))
            throw new UnauthorizedAccessException("Please log in to view notifications.");
        return id;
    }

    public async Task<List<NotificationItem>> GetMineAsync()
    {
        var userId = await GetUserIdAsync();
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Notifications.AsNoTracking()
            .Where(n => n.RecipientUserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc).ThenByDescending(n => n.Id)
            .Select(n => new NotificationItem(
                n.Id, n.Kind, n.PostId, n.CommentId,
                n.ActorUser.UserName ?? "User",
                ((n.ActorUser.FirstName ?? "") + " " + (n.ActorUser.LastName ?? "")).Trim(),
                n.ActorUser.ProfilePicture, n.CreatedAtUtc, n.IsRead))
            .ToListAsync();
    }

    public async Task MarkAllReadAsync()
    {
        var userId = await GetUserIdAsync();
        await using var db = await contextFactory.CreateDbContextAsync();
        await db.Notifications.Where(n => n.RecipientUserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));
    }
}
