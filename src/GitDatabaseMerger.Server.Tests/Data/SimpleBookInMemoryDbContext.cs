using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;

namespace GitDatabaseMerger.Server.Tests.Data
{
    public class SimpleBookInMemoryDbContext : DbContext
    {
        public DbSet<SimpleBook> Books { get; set; }

        public SimpleBookInMemoryDbContext(DbContextOptions<SimpleBookInMemoryDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SimpleBook>().ToTable("Books");
        }
    }
}
