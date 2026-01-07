using Microsoft.EntityFrameworkCore;

namespace RaoVids;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}