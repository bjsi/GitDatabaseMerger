using GitDatabaseMerger.Server.Data;
using GitDatabaseMerger.Server.Tests.Data;
using GitDatabaseMerger.Server.Tests.Helpers;
using GitDatabaseMerger.Server.Tests.Merger;
using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace GitDatabaseMerger.Server.Tests.Tests.Real.Simple
{

    // These three tests cover the simple case where:
    // - There are no merge conflicts to resolve.
    // - The local database can be directly fast-forwarded to the remote database state.
    // For each fast-forward change (row added, row deleted, row changed), a method can be overridden in the Merger
    // to execute some code based on the added/deleted/changed row.

    public class FastForward : RealMergeTestsBase
    {
        [Fact]
        // The local database contains two books.
        // The remote database deletes one of the books.
        // The local database is a direct ancestor of the remote database, therefore a fast-forward merge is possible.
        // There are no merge conflicts to resolve.
        // FastForwardMergeDeletedRow could be overridden in the SimpleMerger to execute some code if necessary based on the deleted row.
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

            await Git.CheckoutBranch(Repo, LocalBranch).ExecuteAsync();

            bool canFastForward = (await Git.CanFastForward(Repo, LocalBranch, RemoteBranch).ExecuteAsync()).ExitCode == 0;
            Assert.True(canFastForward);

            var localPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            var remotePath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            var ancestorPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

            File.Copy(Database, localPath, true);
            File.Copy(Database, ancestorPath, true);

            var mergeResult = await GitMergeAsync();
            Assert.True(mergeResult.ExitCode == 0);
            Assert.Contains("Fast-forward", mergeResult.StandardOutput);

            File.Copy(Database, remotePath, true);

            var (localContext, remoteContext, ancestorContext) = GetDbContexts(localPath, remotePath, ancestorPath);
            var merger = new SimpleMerger(localContext,
                                          remoteContext,
                                          ancestorContext,
                                          x => x.CreatedAt,
                                          x => x.UpdatedAt,
                                          dt,
                                          Server.Models.MergeType.FastForward);
            var res = await merger.MergeAsync();

            var mergedLocalRepo = new GenericRepository<SimpleBook>(new SimpleBookRealDbContext(Database));
            var all = await mergedLocalRepo.GetAll().ToListAsync();
            Assert.Single(all);
            Assert.Equal(all[0].BookId, book1.BookId);
        }

        [Fact]
        // The local database contains one book.
        // The remote database changes one of the properties of the book.
        // The local database is a direct ancestor of the remote database, therefore a fast-forward merge is possible.
        // There are no merge conflicts to resolve.
        // FastForwardMergeChangedRow could be overridden in the SimpleMerger to execute some code if necessary based on the changed row and its properties.
        public async Task TestChangedRow()
        {
            await SetupRepo();

            var dt = DateTime.UtcNow;
            var book1 = new SimpleBook() { Title = "Cool Book", CreatedAt = dt, UpdatedAt = dt };
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

            var localPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            var remotePath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            var ancestorPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

            File.Copy(Database, localPath, true);
            File.Copy(Database, ancestorPath, true);

            var mergeResult = await GitMergeAsync();
            Assert.True(mergeResult.ExitCode == 0);
            Assert.Contains("Fast-forward", mergeResult.StandardOutput);

            File.Copy(Database, remotePath, true);

            var (localContext, remoteContext, ancestorContext) = GetDbContexts(localPath, remotePath, ancestorPath);
            var merger = new SimpleMerger(localContext,
                                          remoteContext,
                                          ancestorContext,
                                          x => x.CreatedAt,
                                          x => x.UpdatedAt,
                                          dt,
                                          Server.Models.MergeType.FastForward);
            var res = await merger.MergeAsync();

            var mergedLocalRepo = new GenericRepository<SimpleBook>(new SimpleBookRealDbContext(Database));
            var all = await mergedLocalRepo.GetAll().ToListAsync();
            Assert.Single(all);
            Assert.Equal(newTitle, all[0].Title);
        }

        [Fact]
        // The local database contains one book.
        // The remote database adds a book.
        // The local database is a direct ancestor of the remote database, therefore a fast-forward merge is possible.
        // There are no merge conflicts to resolve.
        // FastForwardMergeAddedRow could be overridden in the SimpleMerger to execute some code if necessary based on the deleted row.
        public async Task TestRemoteAdddedRow()
        {
            await SetupRepo();

            var dt = DateTime.UtcNow;
            var book1 = new SimpleBook() { Title = "Cool Book", CreatedAt = dt, UpdatedAt = dt };
            var dt2 = dt.AddSeconds(5);
            var book2 = new SimpleBook() { Title = "Cool Book 2", CreatedAt = dt2, UpdatedAt = dt2 };

            var localBooks = new List<SimpleBook> { book1 };
            var remoteBooks = new List<SimpleBook> { book2 };

            await Git.CreateBranchAndCheckout(Repo, LocalBranch).ExecuteAsync();
            await CreateAndCommitAsync(localBooks);

            await Git.CreateBranchAndCheckout(Repo, RemoteBranch).ExecuteAsync();
            await CreateAndCommitAsync(remoteBooks);

            await Git.CheckoutBranch(Repo, LocalBranch).ExecuteAsync();

            bool canFastForward = (await Git.CanFastForward(Repo, LocalBranch, RemoteBranch).ExecuteAsync()).ExitCode == 0;
            Assert.True(canFastForward);

            var localPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            var remotePath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            var ancestorPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

            File.Copy(Database, localPath, true);
            File.Copy(Database, ancestorPath, true);

            var mergeResult = await GitMergeAsync();
            Assert.True(mergeResult.ExitCode == 0);
            Assert.Contains("Fast-forward", mergeResult.StandardOutput);

            File.Copy(Database, remotePath, true);

            var (localContext, remoteContext, ancestorContext) = GetDbContexts(localPath, remotePath, ancestorPath);
            var merger = new SimpleMerger(localContext,
                                          remoteContext,
                                          ancestorContext,
                                          x => x.CreatedAt,
                                          x => x.UpdatedAt,
                                          dt,
                                          Server.Models.MergeType.FastForward);
            var res = await merger.MergeAsync();

            var mergedLocalRepo = new GenericRepository<SimpleBook>(new SimpleBookRealDbContext(Database));
            var all = await mergedLocalRepo.GetAll().ToListAsync();
            Assert.Equal(2, all.Count);
        }
    }
}
