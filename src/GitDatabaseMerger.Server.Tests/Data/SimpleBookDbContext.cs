using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;

namespace GitDatabaseMerger.Server.Tests.Data
{
    public class SimpleBookDbContext : DbContext
    {
        public DbSet<SimpleBook> Books { get; set; }

        public SimpleBookDbContext(DbContextOptions<SimpleBookDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SimpleBook>().ToTable("Books");
        }
    }
}
