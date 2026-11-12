using EventManagement.Context.DataAccess;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace EventManagement.IntegrationTests
{
    public class DatabaseFixture : IAsyncLifetime
    {
        PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").WithDatabase("EventDbTest").Build();
        public async ValueTask InitializeAsync()
        {
            await _postgres.StartAsync();
        }
        public async ValueTask DisposeAsync()
        {
            await _postgres.DisposeAsync();
        }
        public async Task ResetDatabaseAsync()
        {
            NpgsqlConnection.ClearAllPools();
            await using var context = await CreateContextAsync();
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Events\", \"Bookings\" RESTART IDENTITY CASCADE");
            // await context.Database.EnsureDeletedAsync();
            // await context.Database.MigrateAsync();
        }

        public async Task<AppDbContext> CreateContextAsync()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

            var context = new AppDbContext(options);
            await context.Database.MigrateAsync();
            return context;
        }

    }

    [CollectionDefinition("Database")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
