using GitDatabaseMerger.Interop;
using GitDatabaseMerger.Server.Data;
using GitDatabaseMerger.Server.Models;
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

        protected abstract HashSet<string> IgnoreChangedPropertyNames { get; }

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

        private MergeType MergeType { get; }

        public TableMergerBase(DbContext localContext,
                               DbContext remote,
                               DbContext ancestor,
                               Func<TEntity, DateTime> getCreatedAt,
                               Func<TEntity, DateTime> getUpdatedAt,
                               DateTime lastSuccessfulMerge,
                               MergeType mergeType)
        {
            LocalContext = localContext;
            RemoteContext = remote;
            AncestorContext = ancestor;
            GetCreatedAt = getCreatedAt;
            GetUpdatedAt = getUpdatedAt;
            LocalDB = new GenericRepository<TEntity>(LocalContext);
            RemoteDB = new GenericRepository<TEntity>(RemoteContext);
            AncestorDB = new GenericRepository<TEntity>(AncestorContext);
            LastSuccessfulMerge = lastSuccessfulMerge;
            MergeType = mergeType;
        }

        public async Task<MergeResult> MergeAsync()
        {
            var mergeResults = new List<MergeResult>();
            var mergedPrimaryKeys = new KTrie.Trie<object, object[]>();
            foreach (var localRow in LocalDB.GetAll())
            {
                object[] localRowKey = LocalDB.KeysOf(localRow);
                var remoteRowTask = RemoteDB.FindByKeysAsync(localRowKey);
                var ancestorRowTask = AncestorDB.FindByKeysAsync(localRowKey);
                var remoteRow = await remoteRowTask;
                var ancestorRow = await ancestorRowTask;
                var result = await CompareRows(localRow, remoteRow, ancestorRow);
                mergeResults.Add(result);

                if (remoteRow != null)
                    mergedPrimaryKeys.Add(localRowKey, localRowKey);
            }

            foreach (var remoteRow in RemoteDB.GetAll())
            {
                object[] remoteRowKey = RemoteDB.KeysOf(remoteRow);
                if (mergedPrimaryKeys.TryGetValue(remoteRowKey, out _))
                    continue;

                var localRowTask = LocalDB.FindByKeysAsync(remoteRowKey);
                var ancestorRowTask = AncestorDB.FindByKeysAsync(remoteRowKey);
                var localRow = await localRowTask;
                var ancestorRow = await ancestorRowTask;
                var result = await CompareRows(localRow, remoteRow, ancestorRow);
                mergeResults.Add(result);
            }

            return mergeResults.All(x => x == MergeResult.Success)
                ? MergeResult.Success
                : MergeResult.FailedWithUnresolvedConflict;
        }

        private bool RowOnlyInRemote(TEntity localRow, TEntity remoteRow, TEntity ancestorRow)
        {
            return remoteRow != null && localRow == null && ancestorRow == null;
        }

        private bool RowNotInRemote(TEntity localRow, TEntity remoteRow, TEntity ancestorRow)
        {
            return localRow != null && ancestorRow != null && remoteRow == null;
        }

        private bool RowInAll(TEntity localRow, TEntity remoteRow, TEntity ancestorRow)
        {
            return (localRow != null && ancestorRow != null && remoteRow != null);
        }

        private async Task<MergeResult> CompareRows(TEntity localRow, TEntity remoteRow, TEntity ancestorRow)
        {
            // Row exists in the remote DB, but does not exist in the ancestor or local DB
            // The row was created at a time > the last successful merge time.
            if (RowOnlyInRemote(localRow, remoteRow, ancestorRow) && GetCreatedAt(remoteRow) > LastSuccessfulMerge)
            {
                if (MergeType == MergeType.Conflict)
                {
                    return await HandleMergeConflictAddedRow(remoteRow);
                }
                else
                {
                    FastForwardMergeAddedRow(remoteRow);
                    return MergeResult.Success;
                }
            }
            // Row exists in the local and ancestor DBs, but does not exist in the remote DB.
            else if (RowNotInRemote(localRow, remoteRow, ancestorRow))
            {
                if (MergeType == MergeType.Conflict)
                {
                    return await HandleMergeConflictDeletedRow(localRow);
                }
                else
                {
                    FastForwardMergeDeletedRow(localRow);
                    return MergeResult.Success;
                }
            }
            // Row exists in all 3 DBs (local, remote, ancestor)
            // The remote row was updated at a time > the last successful merge time.
            else if (RowInAll(localRow, remoteRow, ancestorRow) && GetUpdatedAt(remoteRow) > LastSuccessfulMerge)
            {
                var changedProperties = GetChangedProperties(localRow, remoteRow);
                if (MergeType == MergeType.Conflict)
                {
                    return await HandleMergeConflictChangedRow(localRow, remoteRow, changedProperties);
                }
                else
                {
                    FastForwardMergeChangedRow(localRow, remoteRow, changedProperties);
                    return MergeResult.Success;
                }
            }
            else
            {
                return await Task.FromResult(MergeResult.Success);
            }
        }

        private IEnumerable<PropertyInfo> GetChangedProperties(TEntity localRow, TEntity remoteRow)
        {
            var changed = new List<PropertyInfo>();
            var properties = localRow.GetType()
                                     .GetProperties()
                                     .Where(x => !IgnoreChangedPropertyNames.Contains(x.Name));
            foreach (var prop in properties)
            {
                var localProp = prop.GetValue(localRow, null);
                var remoteProp = prop.GetValue(remoteRow, null);
                if (!localProp.Equals(remoteProp))
                {
                    changed.Add(prop);
                }
            }
            return changed;
        }

        public virtual void FastForwardMergeAddedRow(TEntity row) { }
        public virtual void FastForwardMergeDeletedRow(TEntity row) { }
        public virtual void FastForwardMergeChangedRow(TEntity from, TEntity to, IEnumerable<PropertyInfo> changedProperties) { }

        /// <summary>
        /// Handle a row that was added to the remote database.
        /// The row exists in the remote DB, but does not exist in the ancestor or local DB.
        /// The row was also created after the last successful merge (ie. it hasn't been handled already).
        /// Default behaviour: Add the row to the local database.
        /// </summary>
        /// <returns>MergeResult indicating whether the merge succeeded or failed.</returns>
        public virtual async Task<MergeResult> HandleMergeConflictAddedRow(TEntity remoteRow)
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
        public virtual async Task<MergeResult> HandleMergeConflictDeletedRow(TEntity localRow)
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
        public virtual async Task<MergeResult> HandleMergeConflictChangedRow(TEntity localRow, TEntity remoteRow, IEnumerable<PropertyInfo> propertyInfos)
        { 
            return (await LocalDB.UpdateAsync(GetUpdatedAt(localRow) > GetUpdatedAt(remoteRow) ? localRow : remoteRow))
                ? MergeResult.Success
                : MergeResult.FailedWithUnresolvedConflict;
        }
    }
}
