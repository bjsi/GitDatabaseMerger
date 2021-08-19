using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace GitDatabaseMerger.Server.Tests.Data
{
    public class TestDbContext : DbContext
    {
        public DbSet<SimpleBook> Books { get; set; }
        // public DbSet<Author> Authors { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>().ToTable("Books");
            modelBuilder.Entity<Author>().ToTable("Authors");
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
