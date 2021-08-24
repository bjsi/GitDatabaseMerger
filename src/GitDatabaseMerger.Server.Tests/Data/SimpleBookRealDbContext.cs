using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;

namespace GitDatabaseMerger.Server.Tests.Data
{
    public class SimpleBookRealDbContext : DbContext
    {
        public DbSet<SimpleBook> Books { get; set; }
        public DbSet<Conflict> Conflicts { get; set; }
        private string DatabasePath { get; }

        public SimpleBookRealDbContext(string databasePath)
        {
            DatabasePath = databasePath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DatabasePath};");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SimpleBook>().ToTable("Books");
            modelBuilder.Entity<Conflict>().ToTable("Conflicts");
        }
    }
}
