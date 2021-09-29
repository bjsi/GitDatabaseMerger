using GitDatabaseMerger.Interop;
using GitDatabaseMerger.Server.Merger;
using GitDatabaseMerger.Server.Models;
using GitDatabaseMerger.Server.TestConsole.Data;
using GitDatabaseMerger.Server.TestConsole.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Server.TestConsole
{
    public class TaskTableMerger : TableMergerBase<MyTask>
    {
        public TaskTableMerger(DbContext local,
                               DbContext remote,
                               DbContext ancestor,
                               Func<MyTask, DateTime> getCreatedAt,
                               Func<MyTask, DateTime> getUpdatedAt,
                               DateTime lastSuccessfulMerge,
                               MergeType mergeType)
            : base(local, remote, ancestor, getCreatedAt, getUpdatedAt, lastSuccessfulMerge, mergeType)
        {
        }

        protected override HashSet<string> IgnoreChangedPropertyNames => throw new NotImplementedException();
    }

    public class DBMerger : IMerger
    {
        public async Task<MergeResult> MergeAsync(string local, string remote, string ancestor)
        {
            var LocalContext = new MyContext(local);
            var RemoteContext = new MyContext(remote);
            var AncestorContext = new MyContext(ancestor);

            //
            // Merge MyTask Table

            var tableMerger = new TaskTableMerger(LocalContext,
                                                  RemoteContext,
                                                  AncestorContext,
                                                  (x) => x.CreatedAt,
                                                  (x) => x.UpdatedAt,
                                                  DateTime.MinValue,
                                                  MergeType.FastForward); // TODO

            var success = await tableMerger.MergeAsync();
            return success;
        }
    }

    class Program
    {
        private const string Hostname = "127.0.0.1";
        private const int Port = 8090;

        static async Task Main(string[] args)
        {
            var merger = new DBMerger();
            using (var listener = new MergeRequestListener(Hostname, Port))
            {
                listener.OnMergeRequest += async (_, args) => await merger.MergeAsync(args.Local, args.Remote, args.Ancestor);
                await listener.StartAsync();
            }
        }
    }
}
