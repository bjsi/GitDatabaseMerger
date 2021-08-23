using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;

namespace GitDatabaseMerger.Server.Tests.Data
{
    public class TestDbContextFactory : IDisposable
    {
        private DbConnection _connection;

        private DbContextOptions<SimpleBookInMemoryDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<SimpleBookInMemoryDbContext>()
                .UseSqlite(_connection).Options;
        }

        public SimpleBookInMemoryDbContext CreateContext()
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                var options = CreateOptions();
                using (var context = new SimpleBookInMemoryDbContext(options))
                {
                    context.Database.EnsureCreated();
                }
            }

            return new SimpleBookInMemoryDbContext(CreateOptions());
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
