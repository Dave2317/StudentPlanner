using Microsoft.EntityFrameworkCore;

namespace StudentPlanner.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<DayEntry> DayEntries { get; set; }
    }
}
