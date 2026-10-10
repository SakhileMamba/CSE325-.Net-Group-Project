namespace SocialMediaApp.Data;

public static class NotificationKinds
{
    public const string PostLike = "PostLike";
    public const string CommentLike = "CommentLike";
}

public class UserNotification
{
    public int Id { get; set; }
    public string RecipientUserId { get; set; } = "";
    public ApplicationUser RecipientUser { get; set; } = null!;
    public string ActorUserId { get; set; } = "";
    public ApplicationUser ActorUser { get; set; } = null!;
    public string Kind { get; set; } = "";
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    public int? CommentId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsRead { get; set; }
}
