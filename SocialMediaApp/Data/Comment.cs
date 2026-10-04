using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Data;

public class Comment
{
    public int Id { get; set; }
    [Required]
    public string Content { get; set; } = "";
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    public int? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
