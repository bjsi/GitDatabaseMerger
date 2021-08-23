using GitDatabaseMerger.Server.Tests.Merger;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GitDatabaseMerger.Server.Tests.Tests.InMemory.OneToMany
{
    public class OneToManyMergeTests : InMemoryMergeTestsBase
    {
        [Fact]
        public async Task TestAllDbsAreEmpty()
        {
            using (var localContext = LocalFactory.CreateContext())
            using (var remoteContext = RemoteFactory.CreateContext())
            using (var ancestorContext = AncestorFactory.CreateContext())
            {
                var merger = new OneToManyMerger(localContext,
                                                  remoteContext,
                                                  ancestorContext,
                                                  x => x.CreatedAt,
                                                  x => x.UpdatedAt,
                                                  DateTime.MinValue);

                var res = await merger.MergeAsync();

                Assert.Equal(Interop.MergeResult.Success, res);
            }
        }
    }
}
