using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VideoApplication.Api.Database.Models;
using VideoApplication.Api.Extensions;

namespace VideoApplication.Api.Database;

public class VideoApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    public DbSet<AccessKey> AccessKeys { get; set; } = null!;

    public DbSet<Channel> Channels { get; set; } = null!;
    public DbSet<Upload> Uploads { get; set; } = null!;
    public DbSet<UploadChunk> UploadChunks { get; set; } = null!;

    public DbSet<Video> Videos { get; set; } = null!;

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

            accessKey.HasIndex(k => k.Value).IncludeProperties(p => p.UserId);
        });

        modelBuilder.Entity<Channel>(channel =>
        {
            channel.HasKey(c => c.Id);
            
            channel.HasIndex(c => c.IdentifierName).IsUnique();
            channel.HasIndex(c => c.DisplayName).IsUnique();
            
            channel.HasOne(c => c.Owner)
                .WithMany(u => u.OwnedChannels)
                .HasPrincipalKey(u => u.Id)
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            channel.HasMany(c => c.Uploads)
                .WithOne(u => u.Channel)
                .HasForeignKey(u => u.ChannelId)
                .HasPrincipalKey(c => c.Id)
                .OnDelete(DeleteBehavior.Cascade);

            channel.HasMany(c => c.Videos)
                .WithOne(v => v.Channel)
                .HasForeignKey(v => v.ChannelId)
                .HasPrincipalKey(c => c.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Upload>(upload =>
        {
            upload.HasKey(u => u.Id);

            upload.HasIndex(u => new {u.ChannelId, u.Sha256Hash}).IsUnique();

            upload.HasMany(u => u.Chunks)
                .WithOne(c => c.Upload)
                .HasForeignKey(c => c.UploadId)
                .HasPrincipalKey(u => u.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UploadChunk>(chunk =>
        {
            chunk.HasKey(c => c.Id);
        });

        modelBuilder.Entity<Video>(video =>
        {
            video.HasKey(v => v.Id);
        });
    }
}