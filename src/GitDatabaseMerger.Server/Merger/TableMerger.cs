using GitDatabaseMerger.Interop;
using GitDatabaseMerger.Server.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Server.Merger
{
    public abstract class TableMergerBase<TEntity> where TEntity : class
    {

        //
        // Database Contexts
        protected DbContext LocalContext { get; }
        protected DbContext RemoteContext { get; }
        protected DbContext AncestorContext { get; }

        //
        // Repositories
        protected GenericRepository<TEntity> LocalDB { get; }
        protected GenericRepository<TEntity> RemoteDB { get; }
        protected GenericRepository<TEntity> AncestorDB { get; }

        private Func<TEntity, DateTime> GetCreatedAt { get; }
        private Func<TEntity, DateTime> GetUpdatedAt { get; }
        private DateTime LastSuccessfulMerge { get; }

        public TableMergerBase(DbContext local,
                               DbContext remote,
                               DbContext ancestor,
                               Func<TEntity, DateTime> getCreatedAt,
                               Func<TEntity, DateTime> getUpdatedAt,
                               DateTime lastSuccessfulMerge)
        {
            LocalContext = local;
            RemoteContext = remote;
            AncestorContext = ancestor;
            GetCreatedAt = getCreatedAt;
            GetUpdatedAt = getUpdatedAt;
            LocalDB = new GenericRepository<TEntity>(LocalContext);
            RemoteDB = new GenericRepository<TEntity>(RemoteContext);
            AncestorDB = new GenericRepository<TEntity>(AncestorContext);
            LastSuccessfulMerge = lastSuccessfulMerge;
        }

        public async Task<MergeResult> Merge()
        {
            var results = new List<MergeResult>();
            var merged = new HashSet<object[]>();
            foreach (var localRow in LocalDB.GetAll())
            {
                object[] localRowKey = LocalDB.KeysOf(localRow);

                var remoteRow = await RemoteDB.FindByKeysAsync(localRowKey);
                var ancestorRow = await AncestorDB.FindByKeysAsync(localRowKey);
                var result = await CompareRows(localRow, remoteRow, ancestorRow);
                merged.Add(RemoteDB.KeysOf(remoteRow));
                results.Add(result);
            }

            foreach (var remoteRow in RemoteDB.GetAll())
            {
                object[] remoteRowKey = RemoteDB.KeysOf(remoteRow);
                if (merged.Contains(remoteRowKey))
                    continue;

                var localRow = await LocalDB.FindByKeysAsync(remoteRowKey);
                var ancestorRow = await LocalDB.FindByKeysAsync(remoteRowKey);
                var result = await CompareRows(localRow, remoteRow, ancestorRow);
                results.Add(result);
            }

            return results.All(x => x == MergeResult.Success)
                ? MergeResult.Success
                : MergeResult.FailedWithUnresolvedConflict;
        }

        private async Task<MergeResult> CompareRows(TEntity localRow, TEntity remoteRow, TEntity ancestorRow)
        {
            // Row exists in the remote DB, but does not exist in the ancestor or local DB
            // The row was created at a time > the last successful merge time.
            if ((remoteRow != null && localRow == null && ancestorRow == null) && GetCreatedAt(remoteRow) > LastSuccessfulMerge)
            {
                return await HandleAddedRow(remoteRow);
            }
            // Row exists in the local and ancestor DBs, but does not exist in the remote DB.
            else if (localRow != null && ancestorRow != null && remoteRow == null)
            {
                return await HandleDeletedRow(localRow);
            }
            // Row exists in all 3 DBs (local, remote, ancestor)
            // The remote row was updated at a time > the last successful merge time.
            else if ((localRow != null && ancestorRow != null && remoteRow != null) && GetUpdatedAt(remoteRow) > LastSuccessfulMerge)
            {
                var propertyInfos = GetChangedProperties(localRow, remoteRow);
                return await HandleChangedRow(localRow, remoteRow, propertyInfos);
            }
            else
            {
                return await Task.FromResult(MergeResult.Success);
            }
        }

        // TODO: probably wrong
        private IEnumerable<PropertyInfo> GetChangedProperties(TEntity localRow, TEntity remoteRow)
        {
            var changed = new List<PropertyInfo>();
            foreach (var prop in localRow.GetType().GetProperties())
            {
                if (prop.GetValue(localRow, null) != prop.GetValue(remoteRow, null))
                {
                    changed.Add(prop);
                }
            }
            return changed;
        }

        /// <summary>
        /// Handle a row that was added to the remote database.
        /// The row exists in the remote DB, but does not exist in the ancestor or local DB.
        /// The row was also created after the last successful merge (ie. it hasn't been handled already).
        /// Default behaviour: Add the row to the local database.
        /// </summary>
        /// <returns>MergeResult indicating whether the merge succeeded or failed.</returns>
        public virtual async Task<MergeResult> HandleAddedRow(TEntity remoteRow)
        {
            return (await LocalDB.CreateAsync(remoteRow))
                ? MergeResult.Success
                : MergeResult.FailedWithUnresolvedConflict;
        }

        /// <summary>
        /// Handle a row that has been deleted in the remote DB.
        /// The row exists in the local and ancestor DBs, but does not exist in the remote DB.
        /// Default behaviour: Delete the row from the local database.
        /// </summary>
        /// <returns>MergeResult indicating whether the merge succeeded or failed.</returns>
        public virtual async Task<MergeResult> HandleDeletedRow(TEntity localRow)
        {
            return (await LocalDB.DeleteAsync(localRow))
                ? MergeResult.Success
                : MergeResult.FailedWithUnresolvedConflict;
        }

        /// <summary>
        /// Handle a row that has different data in the local and remote DBs.
        /// The row exists in the local, remote and ancestor DBs.
        /// The row has different data in the local and remote DBs.
        /// The remote row has been updated since the last successful merge.
        /// Default behaviour: Merge the row that was updated most recently.
        /// </summary>
        /// <returns>MergeResult indicating whether the merge succeeded or failed.</returns>
        public virtual async Task<MergeResult> HandleChangedRow(TEntity localRow, TEntity remoteRow, IEnumerable<PropertyInfo> propertyInfos)
        { 
            return (await LocalDB.UpdateAsync(GetUpdatedAt(localRow) > GetUpdatedAt(remoteRow) ? localRow : remoteRow))
                ? MergeResult.Success
                : MergeResult.FailedWithUnresolvedConflict;
        }
    }
}
