using GitDatabaseMerger.Server.Merger;
using GitDatabaseMerger.Server.Models;
using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace GitDatabaseMerger.Server.Tests.Merger
{
    public class OneToManyMerger : TableMergerBase<AuthorWithBooks>
    {
        public OneToManyMerger(DbContext localContext,
                               DbContext remote,
                               DbContext ancestor,
                               Func<AuthorWithBooks, DateTime> getCreatedAt,
                               Func<AuthorWithBooks, DateTime> getUpdatedAt,
                               DateTime lastSuccessfulMerge,
                               MergeType mergeType)
            : base(localContext, remote, ancestor, getCreatedAt, getUpdatedAt, lastSuccessfulMerge, mergeType)
        {
        }

        protected override HashSet<string> IgnoreChangedPropertyNames => new HashSet<string>
        {
            nameof(AuthorWithBooks.Books), // Ignore navigation property
            nameof(AuthorWithBooks.UpdatedAt),
            nameof(AuthorWithBooks.CreatedAt),
        };
    }
}
