using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;

namespace GitDatabaseMerger.Server.Tests.Data
{
    public class TestDbContextFactory : IDisposable
    {
        private DbConnection _connection;

        private DbContextOptions<SimpleBookDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<SimpleBookDbContext>()
                .UseSqlite(_connection).Options;
        }

        public SimpleBookDbContext CreateContext()
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                var options = CreateOptions();
                using (var context = new SimpleBookDbContext(options))
                {
                    context.Database.EnsureCreated();
                }
            }

            return new SimpleBookDbContext(CreateOptions());
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
