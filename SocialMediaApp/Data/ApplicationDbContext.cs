using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SocialMediaApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
    public DbSet<UserNotification> Notifications => Set<UserNotification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<PostLike>(like =>
        {
            like.HasKey(l => new { l.PostId, l.UserId });
            like.HasOne(l => l.Post).WithMany().HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            like.HasOne(l => l.User).WithMany().HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CommentLike>(like =>
        {
            like.HasKey(l => new { l.CommentId, l.UserId });
            like.HasOne(l => l.Comment).WithMany().HasForeignKey(l => l.CommentId)
                .OnDelete(DeleteBehavior.Cascade);
            like.HasOne(l => l.User).WithMany().HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserNotification>(notification =>
        {
            notification.Property(n => n.Kind).HasMaxLength(20);
            notification.HasIndex(n => new { n.RecipientUserId, n.CreatedAtUtc });
            notification.HasOne(n => n.RecipientUser).WithMany()
                .HasForeignKey(n => n.RecipientUserId).OnDelete(DeleteBehavior.Cascade);
            notification.HasOne(n => n.ActorUser).WithMany()
                .HasForeignKey(n => n.ActorUserId).OnDelete(DeleteBehavior.Cascade);
            notification.HasOne(n => n.Post).WithMany()
                .HasForeignKey(n => n.PostId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Comment>(comment =>
        {
            comment.ToTable("Comments", table => table.HasCheckConstraint(
                "CK_Comments_Content", "length(trim(Content)) > 0"));
            comment.HasOne(c => c.Post).WithMany().HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            comment.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // Preserve other users' replies if an author's account is deleted.
            comment.HasOne(c => c.ParentComment).WithMany().HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Post>(post =>
        {
            post.ToTable("Posts", table => table.HasCheckConstraint(
                "CK_Posts_Content", "length(trim(Content)) > 0 AND length(Content) <= 250"));
            post.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
