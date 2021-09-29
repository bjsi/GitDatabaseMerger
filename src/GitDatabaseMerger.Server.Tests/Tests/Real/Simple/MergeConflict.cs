using GitDatabaseMerger.Server.Data;
using GitDatabaseMerger.Server.Tests.Data;
using GitDatabaseMerger.Server.Tests.Helpers;
using GitDatabaseMerger.Server.Tests.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GitDatabaseMerger.Server.Tests.Tests.Real.Simple
{

    // These three tests cover the case where:
    // - There are merge conflicts to resolve.
    // - The local database CANNOT be directly fast-forwarded to the remote database state.
    // For each conflict (row added, row deleted, row changed), a method can be overridden in the Merger
    // to execute some code based on the added/deleted/changed row, else some default code will be executed
    // to resolve them.

    public class MergeConflict : RealMergeTestsBase
    {
        [Fact]
        public async Task TestRemoteDeletedRow()
        {
            await SetupRepo();

            var dt = DateTime.UtcNow;
            var book1 = new SimpleBook() { Title = "Cool Book", CreatedAt = dt, UpdatedAt = dt };
            var dt2 = dt.AddSeconds(5);
            var book2 = new SimpleBook() { Title = "Cool Book 2", CreatedAt = dt2, UpdatedAt = dt2 };
            var localBooks = new List<SimpleBook> { book1, book2, };

            await Git.CreateBranchAndCheckout(Repo, LocalBranch).ExecuteAsync();
            await CreateAndCommitAsync(localBooks);

            await Git.CreateBranchAndCheckout(Repo, RemoteBranch).ExecuteAsync();
            await DeleteAndCommitAsync(new List<SimpleBook> { book2 });

            // This commit means local is no longer a direct ancestor of remote.
            // There will be a merge conflict.
            await Git.CheckoutBranch(Repo, LocalBranch).ExecuteAsync();
            book2.Title = "Cool Book Changed Title";
            await UpdateAndCommitAsync(new List<SimpleBook> { book2 });

            bool canFastForward = (await Git.CanFastForward(Repo, LocalBranch, RemoteBranch).ExecuteAsync()).ExitCode == 0;
            Assert.False(canFastForward);
        }

        [Fact]
        public async Task TestRemoteAddedRow()
        {
            await SetupRepo();

            var dt = DateTime.UtcNow;
            var book1 = new SimpleBook() { Title = "Cool Book", CreatedAt = dt, UpdatedAt = dt };
            var dt2 = dt.AddSeconds(5);
            var book2 = new SimpleBook() { Title = "Cool Book 2", CreatedAt = dt2, UpdatedAt = dt2 };
            var localBooks = new List<SimpleBook> { book1 };

            await Git.CreateBranchAndCheckout(Repo, LocalBranch).ExecuteAsync();
            await CreateAndCommitAsync(localBooks);

            await Git.CreateBranchAndCheckout(Repo, RemoteBranch).ExecuteAsync();
            await CreateAndCommitAsync(new List<SimpleBook> { book2 });

            // This commit means local is no longer a direct ancestor of remote.
            // There will be a merge conflict.
            await Git.CheckoutBranch(Repo, LocalBranch).ExecuteAsync();
            book1.Title = "Cool Book Changed Title";
            await UpdateAndCommitAsync(new List<SimpleBook> { book1 });

            bool canFastForward = (await Git.CanFastForward(Repo, LocalBranch, RemoteBranch).ExecuteAsync()).ExitCode == 0;
            Assert.False(canFastForward);
        }

        [Fact]
        public async Task TestRemoteChangedRow()
        {
            await SetupRepo();

            var dt = DateTime.UtcNow;
            var book1 = new SimpleBook() { Title = "Cool Book", CreatedAt = dt, UpdatedAt = dt };
            var dt2 = dt.AddSeconds(5);
            var localBooks = new List<SimpleBook> { book1 };

            await Git.CreateBranchAndCheckout(Repo, LocalBranch).ExecuteAsync();
            await CreateAndCommitAsync(localBooks);

            await Git.CreateBranchAndCheckout(Repo, RemoteBranch).ExecuteAsync();
            var remoteRepo = new GenericRepository<SimpleBook>(new SimpleBookRealDbContext(Database));
            var book1Remote = await remoteRepo.FindByKeysAsync(1);
            var newTitle = "Cool Book Changed Title";
            book1Remote.Title = newTitle;
            book1Remote.UpdatedAt = dt.AddSeconds(6);
            await UpdateAndCommitAsync(new List<SimpleBook> { book1Remote });

            await Git.CheckoutBranch(Repo, LocalBranch).ExecuteAsync();

            bool canFastForward = (await Git.CanFastForward(Repo, LocalBranch, RemoteBranch).ExecuteAsync()).ExitCode == 0;
            Assert.True(canFastForward);
        }
    }
}
