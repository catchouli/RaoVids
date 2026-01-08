using Microsoft.EntityFrameworkCore;
using RaoVids.Models;

namespace RaoVids;

public class AppDbContext : DbContext
{
    public DbSet<Channel> Channels { get; set; }

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
    }
}