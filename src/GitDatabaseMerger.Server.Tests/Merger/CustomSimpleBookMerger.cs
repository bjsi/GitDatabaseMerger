using GitDatabaseMerger.Interop;
using GitDatabaseMerger.Server.Merger;
using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Server.Tests.Merger
{
    public class CustomSimpleBookMerger : TableMergerBase<SimpleBook>
    {
        public CustomSimpleBookMerger(DbContext localContext,
                                      DbContext remote,
                                      DbContext ancestor,
                                      Func<SimpleBook, DateTime> getCreatedAt,
                                      Func<SimpleBook, DateTime> getUpdatedAt,
                                      DateTime lastSuccessfulMerge)
            : base(localContext, remote, ancestor, getCreatedAt, getUpdatedAt, lastSuccessfulMerge)
        {
        }

        protected override HashSet<string> IgnoreChangedPropertyNames { get; } = new HashSet<string>
        {
            nameof(SimpleBook.CreatedAt),
            nameof(SimpleBook.UpdatedAt),
        };

        public override async Task<MergeResult> HandleChanged(SimpleBook localRow, SimpleBook remoteRow, IEnumerable<PropertyInfo> propertyInfos)
        {
            foreach (var prop in propertyInfos)
            {
                if (prop.Name == "Title")
                {
                    return await HandleChangedTitle(localRow, remoteRow);
                }
            }

            return await Task.FromResult(MergeResult.FailedWithUnresolvedConflict);
        }

        private async Task<MergeResult> HandleChangedTitle(SimpleBook localRow, SimpleBook remoteRow)
        {
            var title = localRow.Title.Length > remoteRow.Title.Length
                ? localRow.Title
                : remoteRow.Title;
            localRow.Title = title;

            return (await LocalDB.UpdateAsync(localRow))
                ? MergeResult.Success
                : MergeResult.FailedWithUnresolvedConflict;
        }
    }
}
