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

        //public override int SaveChanges()
        //{
        //    var entries = ChangeTracker
        //        .Entries()
        //        .Where(e => e.Entity is BaseEntity && (
        //                e.State == EntityState.Added
        //                || e.State == EntityState.Modified));

        //    foreach (var entityEntry in entries)
        //    {
        //        ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;

        //        if (entityEntry.State == EntityState.Added)
        //        {
        //            ((BaseEntity)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
        //        }
        //    }

        //    return base.SaveChanges();
        //}
    }
}
