using Microsoft.EntityFrameworkCore;
using RaoVids.Models;

namespace RaoVids;

public class AppDbContext : DbContext
{
    public DbSet<Channel> Channels { get; set; }
    public DbSet<Video> Videos { get; set; }
    public DbSet<LogMessage> LogMessages { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Channel>()
            .Property(c => c.ChannelId)
            .IsRequired();

        modelBuilder.Entity<Channel>()
            .HasIndex(c => c.ChannelId)
            .IsUnique();

        modelBuilder.Entity<Channel>()
            .Property(c => c.ChannelName)
            .IsRequired();

        modelBuilder.Entity<Video>()
            .Property(v => v.VideoId)
            .IsRequired();

        modelBuilder.Entity<Video>()
            .HasIndex(v => v.VideoId)
            .IsUnique();

        modelBuilder.Entity<Video>()
            .Property(v => v.ChannelId)
            .IsRequired();

        modelBuilder.Entity<Video>()
            .Property(v => v.Title)
            .IsRequired();

        modelBuilder.Entity<Video>()
            .HasIndex(v => v.ChannelId);

        modelBuilder.Entity<Video>()
            .HasIndex(v => v.PublishedAt);
    }
}