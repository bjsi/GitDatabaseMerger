using GitDatabaseMerger.Server.Merger;
using GitDatabaseMerger.Server.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Server.Tests.Merger
{
    public class SimpleBookMerger : TableMergerBase<SimpleBook>
    {

        public SimpleBookMerger(DbContext local,
                               DbContext remote,
                               DbContext ancestor,
                               Func<SimpleBook, DateTime> getCreatedAt,
                               Func<SimpleBook, DateTime> getUpdatedAt,
                               DateTime lastSuccessfulMerge)
            : base(local, remote, ancestor, getCreatedAt, getUpdatedAt, lastSuccessfulMerge)
        {
        }
    }
}
