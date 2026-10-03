using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Data;

public class Post
{
    public int Id { get; set; }

    [Required]
    [StringLength(250)]
    public string Content { get; set; } = "";

    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
}
