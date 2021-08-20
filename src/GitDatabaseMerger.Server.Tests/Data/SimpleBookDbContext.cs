using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

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
