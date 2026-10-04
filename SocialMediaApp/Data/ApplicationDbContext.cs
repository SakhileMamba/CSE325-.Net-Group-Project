using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SocialMediaApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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
