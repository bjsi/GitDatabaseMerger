using GitDatabaseMerger.Server.Data;
using GitDatabaseMerger.Server.Tests.Merger;
using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GitDatabaseMerger.Server.Tests.Tests.InMemory.Simple
{
    public class CustomSimpleMergeTests : MergeTestsBase
    {
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
                var merger = new CustomSimpleMerger(localContext,
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

    }
}
