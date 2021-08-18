using CliWrap;
using GitDatabaseMerger.Server.Tests.Data;
using GitDatabaseMerger.Server.Tests.Helpers;
using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace GitDatabaseMerger.Server.Tests
{
    public class InMemoryMergeTests
    {
        [Fact]
        public async Task TestAddedRow()
        {
            using (var factory = new SampleDbContextFactory())
            {
                // Get a context
                using (var context = factory.CreateContext())
                {
                    var dt = DateTime.UtcNow;
                    var user = new SimpleBook() { Title = "Cool Book", CreatedAt = dt, UpdatedAt = dt};
                    context.Books.Add(user);
                    await context.SaveChangesAsync();
                }

                // Get another context using the same connection
                using (var context = factory.CreateContext())
                {
                    var count = await context.Books.CountAsync();
                    Assert.Equal(1, count);

                    var b = await context.Books.FirstOrDefaultAsync(book => book.Title == "Cool Book");
                    Assert.NotNull(b);
                }
            }
        }
    }
}
