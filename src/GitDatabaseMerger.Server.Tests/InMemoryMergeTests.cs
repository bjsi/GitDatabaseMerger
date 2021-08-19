using GitDatabaseMerger.Server.Data;
using GitDatabaseMerger.Server.Tests.Data;
using GitDatabaseMerger.Server.Tests.Merger;
using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GitDatabaseMerger.Server.Tests
{
    public class InMemoryMergeTests
    {
        [Fact]
        public async Task TestAllDbsAreEmpty()
        {
            using (var localFactory = new TestDbContextFactory())
            using (var remoteFactory = new TestDbContextFactory())
            using (var ancestorFactory = new TestDbContextFactory())
            {
                using (var localContext = localFactory.CreateContext())
                using (var remoteContext = remoteFactory.CreateContext())
                using (var ancestorContext = ancestorFactory.CreateContext())
                {
                    var merger = new SimpleBookMerger(localContext,
                                                      remoteContext,
                                                      ancestorContext,
                                                      x => x.CreatedAt,
                                                      x => x.UpdatedAt,
                                                      DateTime.MinValue);

                    var res = await merger.Merge();

                    Assert.Equal(Interop.MergeResult.Success, res);
                }
            }
        }

        [Fact]
        public async Task TestRemoteAddedRow()
        {
            var dt = DateTime.UtcNow;
            var book1 = new SimpleBook() { Title = "Cool Book", CreatedAt = dt, UpdatedAt = dt };

            var dt2 = dt.AddSeconds(5);
            var book2 = new SimpleBook() { Title = "Cool Book 2", CreatedAt = dt2, UpdatedAt = dt2 };

            using (var localFactory = new TestDbContextFactory())
            using (var remoteFactory = new TestDbContextFactory())
            using (var ancestorFactory = new TestDbContextFactory())
            {
                using (var context = localFactory.CreateContext())
                {
                    var repo = new GenericRepository<SimpleBook>(context);
                    await repo.CreateAsync(book1);
                }

                using (var context = remoteFactory.CreateContext())
                {
                    var repo = new GenericRepository<SimpleBook>(context);
                    await repo.CreateAsync(book1);
                    await repo.CreateAsync(book2);
                }

                using (var context = ancestorFactory.CreateContext())
                {
                    var repo = new GenericRepository<SimpleBook>(context);
                    await repo.CreateAsync(book1);
                }

                using (var localContext = localFactory.CreateContext())
                using (var remoteContext = remoteFactory.CreateContext())
                using (var ancestorContext = ancestorFactory.CreateContext())
                {
                    var merger = new SimpleBookMerger(localContext,
                                                      remoteContext,
                                                      ancestorContext,
                                                      x => x.CreatedAt,
                                                      x => x.UpdatedAt,
                                                      dt);

                    var res = await merger.Merge();
                    Assert.Equal(Interop.MergeResult.Success, res);

                    var repo = new GenericRepository<SimpleBook>(localContext);
                    var all = await repo.GetAll().ToListAsync();
                    Assert.Equal(2, all.Count);
                }
            }
        }

        [Fact]
        public async Task TestAllDbsAreTheSame()
        {
            var dt = DateTime.UtcNow;
            var book = new SimpleBook() { Title = "Cool Book", CreatedAt = dt, UpdatedAt = dt };

            using (var localFactory = new TestDbContextFactory())
            using (var remoteFactory = new TestDbContextFactory())
            using (var ancestorFactory = new TestDbContextFactory())
            {

                using (var context = localFactory.CreateContext())
                {
                    var repo = new GenericRepository<SimpleBook>(context);
                    await repo.CreateAsync(book);
                }

                using (var context = remoteFactory.CreateContext())
                {
                    var repo = new GenericRepository<SimpleBook>(context);
                    await repo.CreateAsync(book);
                }

                using (var context = ancestorFactory.CreateContext())
                {
                    var repo = new GenericRepository<SimpleBook>(context);
                    await repo.CreateAsync(book);
                }

                using (var localContext = localFactory.CreateContext())
                using (var remoteContext = remoteFactory.CreateContext())
                using (var ancestorContext = ancestorFactory.CreateContext())
                {
                    var merger = new SimpleBookMerger(localContext,
                                                      remoteContext,
                                                      ancestorContext,
                                                      x => x.CreatedAt,
                                                      x => x.UpdatedAt,
                                                      dt);

                    var res = await merger.Merge();
                    Assert.Equal(Interop.MergeResult.Success, res);
                }
            }
        }
    }
}
