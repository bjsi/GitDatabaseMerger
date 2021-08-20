using GitDatabaseMerger.Server.Data;
using GitDatabaseMerger.Server.Tests.Merger;
using GitDatabaseMerger.Server.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GitDatabaseMerger.Server.Tests
{
    public class InMemoryCustomSimpleBookMergeTests : MergeTestsBase
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
                var merger = new CustomSimpleBookMerger(localContext,
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
