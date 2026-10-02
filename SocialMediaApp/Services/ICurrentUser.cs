namespace SocialMediaApp.Services;

/// <summary>Read-only identity information for profile, posts, and other feature components.</summary>
public interface ICurrentUser
{
    Task<CurrentUserInfo?> GetAsync();
}

public sealed record CurrentUserInfo(string Id, string? Email);
