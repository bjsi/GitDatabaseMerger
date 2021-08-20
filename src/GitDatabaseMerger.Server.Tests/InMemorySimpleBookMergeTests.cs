using GitDatabaseMerger.Server.Data;
using GitDatabaseMerger.Server.Tests.Data;
using GitDatabaseMerger.Server.Tests.Merger;
using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GitDatabaseMerger.Server.Tests
{
    public class InMemorySimpleBookMergeTests : MergeTestsBase
    {
        [Fact]
        public async Task TestAllDbsAreEmpty()
        {
            using (var localContext = LocalFactory.CreateContext())
            using (var remoteContext = RemoteFactory.CreateContext())
            using (var ancestorContext = AncestorFactory.CreateContext())
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

        [Fact]
        public async Task TestRemoteDeletedRow()
        {
            var dt = DateTime.UtcNow;
            var book1 = new SimpleBook() { Title = "Cool Book", CreatedAt = dt, UpdatedAt = dt };

            var dt2 = dt.AddSeconds(5);
            var book2 = new SimpleBook() { Title = "Cool Book 2", CreatedAt = dt2, UpdatedAt = dt2 };

            var local = new List<SimpleBook> { book1, book2, };
            var remote = new List<SimpleBook> { book1 };
            var ancestor = local;

            await CreateDBsAsync(local, remote, ancestor);

            using (var localContext = LocalFactory.CreateContext())
            using (var remoteContext = RemoteFactory.CreateContext())
            using (var ancestorContext = AncestorFactory.CreateContext())
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
                Assert.Single(all);
            }
        }

        [Fact]
        public async Task TestRemoteAddedRow()
        {
            var dt = DateTime.UtcNow;
            var book1 = new SimpleBook() { Title = "Cool Book", CreatedAt = dt, UpdatedAt = dt };

            var dt2 = dt.AddSeconds(5);
            var book2 = new SimpleBook() { Title = "Cool Book 2", CreatedAt = dt2, UpdatedAt = dt2 };

            var local = new List<SimpleBook> { book1 };
            var remote = new List<SimpleBook> { book1, book2 };
            var ancestor = local;

            await CreateDBsAsync(local, remote, ancestor);

            using (var localContext = LocalFactory.CreateContext())
            using (var remoteContext = RemoteFactory.CreateContext())
            using (var ancestorContext = AncestorFactory.CreateContext())
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

        [Fact]
        public async Task TestChangedRow()
        {
            var dt = DateTime.UtcNow;
            var book1 = new SimpleBook() { Title = "Cool Book", CreatedAt = dt, UpdatedAt = dt };
            var book2 = new SimpleBook() { Title = "Cool Book is Cool", CreatedAt = dt, UpdatedAt = dt.AddSeconds(5) };

            var local = new List<SimpleBook> { book1 };
            var remote = new List<SimpleBook> { book2 };
            var ancestor = local;

            await CreateDBsAsync(local, remote, ancestor);

            using (var localContext = LocalFactory.CreateContext())
            using (var remoteContext = RemoteFactory.CreateContext())
            using (var ancestorContext = AncestorFactory.CreateContext())
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
                Assert.Single(all);

                var fst = all[0];
                Assert.Equal(book2.Title, fst.Title);
            }
        }

        [Fact]
        public async Task TestAllDbsAreTheSame()
        {
            var dt = DateTime.UtcNow;
            var book = new SimpleBook() { Title = "Cool Book", CreatedAt = dt, UpdatedAt = dt };

            var local = new List<SimpleBook> { book };
            var remote = local;
            var ancestor = local;

            await CreateDBsAsync(local, remote, ancestor);

            using (var localContext = LocalFactory.CreateContext())
            using (var remoteContext = RemoteFactory.CreateContext())
            using (var ancestorContext = AncestorFactory.CreateContext())
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
