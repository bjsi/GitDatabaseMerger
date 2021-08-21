using GitDatabaseMerger.Server.Merger;
using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace GitDatabaseMerger.Server.Tests.Merger
{
    public class ManyToOneMerger : TableMergerBase<BookWithAuthor>
    {
        public ManyToOneMerger(DbContext localContext,
                               DbContext remote,
                               DbContext ancestor,
                               Func<BookWithAuthor, DateTime> getCreatedAt,
                               Func<BookWithAuthor, DateTime> getUpdatedAt,
                               DateTime lastSuccessfulMerge)
            : base(localContext, remote, ancestor, getCreatedAt, getUpdatedAt, lastSuccessfulMerge)
        {
        }

        protected override HashSet<string> IgnoreChangedPropertyNames => new HashSet<string> 
        { 
            nameof(BookWithAuthor.Author), // Ignore navigation property
            nameof(BookWithAuthor.CreatedAt),
            nameof(BookWithAuthor.UpdatedAt),
        };
    }
}
