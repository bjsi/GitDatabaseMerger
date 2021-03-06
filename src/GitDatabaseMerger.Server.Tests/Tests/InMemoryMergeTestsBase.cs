using GitDatabaseMerger.Server.Data;
using GitDatabaseMerger.Server.Tests.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Server.Tests.Tests
{
    public abstract class InMemoryMergeTestsBase : IDisposable
    {

        protected TestDbContextFactory LocalFactory { get; }
        protected TestDbContextFactory RemoteFactory { get; }
        protected TestDbContextFactory AncestorFactory { get; }

        public InMemoryMergeTestsBase()
        {
            LocalFactory = new TestDbContextFactory();
            RemoteFactory = new TestDbContextFactory();
            AncestorFactory = new TestDbContextFactory();
        }

        public virtual void Dispose()
        {
            LocalFactory.Dispose();
            RemoteFactory.Dispose();
            AncestorFactory.Dispose();
        }

        public async Task CreateAsync<T>(TestDbContextFactory factory, List<T> list) where T : class
        {
            using (var context = factory.CreateContext())
            {
                var repo = new GenericRepository<T>(context);
                foreach (var book in list)
                    await repo.CreateAsync(book);
            }
        }

        public async Task CreateDBsAsync<T>(List<T> local, List<T> remote, List<T> ancestor) where T : class
        {
            await Task.WhenAll(CreateAsync(LocalFactory, local),
                               CreateAsync(RemoteFactory, remote),
                               CreateAsync(AncestorFactory, ancestor));
        }
    }
}