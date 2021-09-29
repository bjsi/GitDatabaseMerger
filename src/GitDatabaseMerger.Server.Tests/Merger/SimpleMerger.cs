using GitDatabaseMerger.Server.Merger;
using GitDatabaseMerger.Server.Models;
using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace GitDatabaseMerger.Server.Tests.Merger
{
    public class SimpleMerger : TableMergerBase<SimpleBook>
    {

        public SimpleMerger(DbContext local,
                            DbContext remote,
                            DbContext ancestor,
                            Func<SimpleBook, DateTime> getCreatedAt,
                            Func<SimpleBook, DateTime> getUpdatedAt,
                            DateTime lastSuccessfulMerge,
                            MergeType mergeType)
            : base(local, remote, ancestor, getCreatedAt, getUpdatedAt, lastSuccessfulMerge, mergeType)
        {
        }

        protected override HashSet<string> IgnoreChangedPropertyNames { get; } = new HashSet<string>
        {
            nameof(SimpleBook.CreatedAt),
            nameof(SimpleBook.UpdatedAt),
        };
    }
}
