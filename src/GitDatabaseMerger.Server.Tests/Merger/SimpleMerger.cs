using GitDatabaseMerger.Server.Merger;
using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GitDatabaseMerger.Server.Tests.Merger
{
    public class SimpleMerger : TableMergerBase<SimpleBook>
    {

        public SimpleMerger(DbContext local,
                               DbContext remote,
                               DbContext ancestor,
                               Func<SimpleBook, DateTime> getCreatedAt,
                               Func<SimpleBook, DateTime> getUpdatedAt,
                               DateTime lastSuccessfulMerge)
            : base(local, remote, ancestor, getCreatedAt, getUpdatedAt, lastSuccessfulMerge)
        {
        }

        protected override HashSet<string> IgnoreChangedPropertyNames { get; } = new HashSet<string>
        {
            nameof(SimpleBook.CreatedAt),
            nameof(SimpleBook.UpdatedAt),
        };
    }
}
