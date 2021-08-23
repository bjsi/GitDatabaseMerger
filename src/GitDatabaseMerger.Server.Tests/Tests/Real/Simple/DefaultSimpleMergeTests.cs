using CliWrap;
using CliWrap.Buffered;
using GitDatabaseMerger.Server.Data;
using GitDatabaseMerger.Server.Tests.Helpers;
using GitDatabaseMerger.Server.Tests.Merger;
using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace GitDatabaseMerger.Server.Tests.Tests.Real.Simple
{
    public class DefaultSimpleMergeTests : RealMergeTestsBase
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

            await Git.CreateBranchAndCheckout(Repo, LocalBranchName).ExecuteAsync();
            await CreateAsync(localBooks);

            await Git.CreateBranchAndCheckout(Repo, RemoteBranchName).ExecuteAsync();
            await DeleteAsync(new List<SimpleBook> { book2 });

            await Git.CheckoutBranch(Repo, LocalBranchName).ExecuteAsync();
            var info = new ProcessStartInfo();
            var path = info.EnvironmentVariables["PATH"];
            var mergeResult = await Git.Merge(Repo, RemoteBranchName)
                                       .WithValidation(CommandResultValidation.None)
                                       .WithEnvironmentVariables((b) => b.Set("PATH", path + ";" + ScriptsPath))
                                       .ExecuteBufferedAsync();
            var (localPath, remotePath, ancestorPath) = GetDatabaseFiles(mergeResult.StandardOutput);
            var (localContext, remoteContext, ancestorContext) = GetDbContexts(localPath, remotePath, ancestorPath);
            var merger = new SimpleMerger(localContext,
                                          remoteContext,
                                          ancestorContext,
                                          (x) => x.CreatedAt,
                                          (x) => x.UpdatedAt,
                                          dt);

            var res = await merger.MergeAsync();
            var localRep = new GenericRepository<SimpleBook>(localContext);
            var all = await localRep.GetAll().ToListAsync();
            Assert.Single(all);
        }

        [Fact]
        public async Task TestRemoteAdddedRow()
        {
            await SetupRepo();

            var dt = DateTime.UtcNow;
            var book1 = new SimpleBook() { Title = "Cool Book", CreatedAt = dt, UpdatedAt = dt };

            var dt2 = dt.AddSeconds(5);
            var book2 = new SimpleBook() { Title = "Cool Book 2", CreatedAt = dt2, UpdatedAt = dt2 };

            var localBooks = new List<SimpleBook> { book1 };
            var remoteBooks = new List<SimpleBook> { book2 };

            await Git.CreateBranchAndCheckout(Repo, LocalBranchName).ExecuteAsync();
            await CreateAsync(localBooks);

            await Git.CreateBranchAndCheckout(Repo, RemoteBranchName).ExecuteAsync();
            await DeleteAsync(remoteBooks);

            await Git.CheckoutBranch(Repo, LocalBranchName).ExecuteAsync();
            var info = new ProcessStartInfo();
            var path = info.EnvironmentVariables["PATH"];
            var mergeResult = await Git.Merge(Repo, RemoteBranchName)
                                       .WithValidation(CommandResultValidation.None)
                                       .WithEnvironmentVariables((b) => b.Set("PATH", path + ";" + ScriptsPath))
                                       .ExecuteBufferedAsync();

            var (localPath, remotePath, ancestorPath) = GetDatabaseFiles(mergeResult.StandardOutput);
            var (localContext, remoteContext, ancestorContext) = GetDbContexts(localPath, remotePath, ancestorPath);
            var merger = new SimpleMerger(localContext,
                                          remoteContext,
                                          ancestorContext,
                                          (x) => x.CreatedAt,
                                          (x) => x.UpdatedAt,
                                          dt);

            var res = await merger.MergeAsync();
            var localRep = new GenericRepository<SimpleBook>(localContext);
            var all = await localRep.GetAll().ToListAsync();
            Assert.Equal(2, all.Count);
        }
    }
}
