namespace SocialMediaApp.Data;

public class CommentLike
{
    public int CommentId { get; set; }
    public Comment Comment { get; set; } = null!;
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
}
