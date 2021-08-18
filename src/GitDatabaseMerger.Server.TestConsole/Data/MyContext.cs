using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Server.TestConsole.Data
{
    public class MyContext : DbContext
    {
        public DbSet<Task> Tasks { get; set; }
        private string DatabasePath { get; }

        public MyContext(string databasePath)
        {
            DatabasePath = databasePath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DatabasePath};");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Task>().ToTable("Tasks");
        }
    }
}
