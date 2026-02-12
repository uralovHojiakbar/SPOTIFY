using Microsoft.EntityFrameworkCore;
using SPOTIFY.Models;

namespace SPOTIFY.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<LocaleEntry> LocaleEntries => Set<LocaleEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocaleEntry>()
            .HasIndex(x => new { x.Locale, x.Kind });
    }
}
