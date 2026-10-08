using Microsoft.AspNetCore.Identity;

namespace SocialMediaApp.Data;

// Stores additional profile information for each application user.
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Bio { get; set; }

    public string? Interests { get; set; }

    public string? ProfilePicture { get; set; }
}