using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Server.Tests.Data
{
    public class SampleDbContextFactory : IDisposable
    {
        private DbConnection _connection;

        private DbContextOptions<MyContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<MyContext>()
                .UseSqlite(_connection).Options;
        }

        public MyContext CreateContext()
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                var options = CreateOptions();
                using (var context = new MyContext(options))
                {
                    context.Database.EnsureCreated();
                }
            }

            return new MyContext(CreateOptions());
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
