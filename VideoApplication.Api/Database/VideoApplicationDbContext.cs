using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VideoApplication.Api.Database.Models;

namespace VideoApplication.Api.Database;

public class VideoApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    public DbSet<AccessKey> AccessKeys { get; set; } = null!;

    public VideoApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AccessKey>(accessKey =>
        {
            accessKey.HasKey(k => k.Id);
            accessKey.HasOne(k => k.User)
                .WithMany(u => u.AccessKeys)
                .HasPrincipalKey(u => u.Id)
                .HasForeignKey(k => k.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            accessKey.HasIndex(k => k.Value);
        });
    }
}