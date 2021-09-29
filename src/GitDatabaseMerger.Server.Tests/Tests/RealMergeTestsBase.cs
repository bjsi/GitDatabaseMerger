using CliWrap;
using CliWrap.Buffered;
using GitDatabaseMerger.Server.Data;
using GitDatabaseMerger.Server.Tests.Data;
using GitDatabaseMerger.Server.Tests.Helpers;
using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Server.Tests.Tests
{
    public abstract class RealMergeTestsBase : IDisposable
    {
        protected string Repo { get; }
        protected string Database { get; }
        protected string RelDbPath { get; }
        protected string LocalBranch { get; }  = "localbranch";
        protected string RemoteBranch { get; } = "remotebranch";
        private static string BaseDir { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        protected static string ScriptsPath { get; } = Path.Combine(BaseDir, "Scripts");
        protected static string ScriptName { get; } = "mergedriver.ps1";
        protected string MergeDriverScript { get; } = Path.Combine(ScriptsPath, ScriptName);
             
        public RealMergeTestsBase()
        {
            Repo = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(Repo);
            RelDbPath = Path.GetRandomFileName();
            Database = Path.Combine(Repo, RelDbPath);
        }

        protected async Task CreateConflictToGuaranteeMergeDriver()
        {
            var dt = DateTime.UtcNow;
            await CreateAndCommitAsync(new List<Conflict> { new Conflict { CreatedAt = dt, UpdatedAt = dt } });
        }

        protected async Task<BufferedCommandResult> GitMergeAsync()
        {
            return await Git.Merge(Repo, RemoteBranch)
                              .WithValidation(CommandResultValidation.None)
                              .ExecuteBufferedAsync();
        }


        protected async Task SetupRepo()
        {
            await Git.Init(Repo).ExecuteAsync();
            await Git.SetGitAttributes(Repo, $"{RelDbPath} diff=sqlite merge=sqlite");
            await Git.SetConfigValue(Repo, "merge.sqlite.name", "sqlite merge").ExecuteAsync();
            // Merge driver script copies the .merge_files to temporary files,
            // writes the locations of the copied files to stdout and exits with
            // status code 1 (merge conflict). This allows you to merge access the merge files in the
            // table merger tests and commit the changes to resolve the merge.
            await Git.SetConfigValue(Repo, "merge.sqlite.driver", $@"powershell {ScriptName} -local %A -remote %B -ancestor %O").ExecuteAsync();
            await Git.SetConfigValue(Repo, "diff.sqlite.binary", "true").ExecuteAsync();
            await Git.AddAll(Repo).ExecuteAsync();
            await Git.Commit(Repo).ExecuteAsync();
        }

        public (DbContext, DbContext, DbContext) GetDbContexts(string local, string remote, string ancestor)
        {
            return (new SimpleBookRealDbContext(local), new SimpleBookRealDbContext(remote), new SimpleBookRealDbContext(ancestor));
        }

        public (string, string, string) GetDatabaseFiles(string stdout)
        {
            var files = stdout.Split(Environment.NewLine)[0]
                              .Split(" ")
                              .Select(x => Path.Combine(Repo, x))
                              .Where(x => File.Exists(x))
                              .ToArray();
            return files.Length == 3
                ? (files[0], files[1], files[2])
                : (null, null, null);
        }

        public void DeleteDirectory(string targetDir)
        {
            File.SetAttributes(targetDir, FileAttributes.Normal);

            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }

        public void Dispose()
        {
            DeleteDirectory(Repo);
        }

        public async Task DeleteAsync<T>(List<T> list) where T : class
        {
            using (var context = new SimpleBookRealDbContext(Database))
            {
                context.Database.EnsureCreated();
                var repo = new GenericRepository<T>(context);
                foreach (var book in list)
                    await repo.DeleteAsync(book);
            }
            await Git.AddAll(Repo).ExecuteAsync();
            await Git.Commit(Repo).ExecuteAsync();
        }

        public async Task UpdateAndCommitAsync<T>(List<T> list) where T : class
        {
            using (var context = new SimpleBookRealDbContext(Database))
            {
                context.Database.EnsureCreated();
                var repo = new GenericRepository<T>(context);
                foreach (var book in list)
                    await repo.UpdateAsync(book);
            }
            await Git.AddAll(Repo).ExecuteAsync();
            await Git.Commit(Repo).ExecuteAsync();
        }

        public async Task CreateAndCommitAsync<T>(List<T> list) where T : class
        {
            using (var context = new SimpleBookRealDbContext(Database))
            {
                context.Database.EnsureCreated();
                var repo = new GenericRepository<T>(context);
                foreach (var book in list)
                    await repo.CreateAsync(book);
            }
            await Git.AddAll(Repo).ExecuteAsync();
            await Git.Commit(Repo).ExecuteAsync();
        }
    }
}
