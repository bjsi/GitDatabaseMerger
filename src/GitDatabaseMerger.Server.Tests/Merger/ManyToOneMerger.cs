using GitDatabaseMerger.Server.Merger;
using GitDatabaseMerger.Server.Models;
using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GitDatabaseMerger.Server.Tests.Merger
{
    public class ManyToOneMerger : TableMergerBase<BookWithAuthor>
    {
        public ManyToOneMerger(DbContext localContext,
                               DbContext remote,
                               DbContext ancestor,
                               Func<BookWithAuthor, DateTime> getCreatedAt,
                               Func<BookWithAuthor, DateTime> getUpdatedAt,
                               DateTime lastSuccessfulMerge,
                               MergeType mergeType)
            : base(localContext, remote, ancestor, getCreatedAt, getUpdatedAt, lastSuccessfulMerge, mergeType)
        {
        }

        protected override HashSet<string> IgnoreChangedPropertyNames => new HashSet<string> 
        { 
            nameof(BookWithAuthor.Author), // Ignore navigation property
            nameof(BookWithAuthor.CreatedAt),
            nameof(BookWithAuthor.UpdatedAt),
        };

        public override void FastForwardMergeAddedRow(BookWithAuthor row)
        {
        }

        public override void FastForwardMergeChangedRow(BookWithAuthor from, BookWithAuthor to, IEnumerable<PropertyInfo> changedProperties)
        {
        }

        public override void FastForwardMergeDeletedRow(BookWithAuthor row)
        {
        }
    }
}
