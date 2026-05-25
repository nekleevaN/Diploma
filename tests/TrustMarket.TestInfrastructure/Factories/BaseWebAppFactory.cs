using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TrustMarket.TestInfrastructure.Helpers;

namespace TrustMarket.TestInfrastructure.Factories;

public abstract class BaseWebAppFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("MassTransit:UseInMemory", "true");
        builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");
        builder.UseSetting("SkipStartupMigration", "true");
        builder.ConfigureTestServices(services =>
        {
            RemoveBackgroundServices(services);
            ConfigureTestServices(services);
        });
    }

    protected virtual void RemoveBackgroundServices(IServiceCollection services) { }
    protected virtual void ConfigureTestServices(IServiceCollection services) { }

    protected void UseSqliteFor<TDbContext>(IServiceCollection services) where TDbContext : DbContext
    {
        if (_connection == null)
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();
        }
        services.RemoveAll<DbContextOptions<TDbContext>>();
        services.AddDbContext<TDbContext>(options =>
            options.UseSqlite(_connection)
                   .ReplaceService<IModelCustomizer, SqliteModelCustomizer>());
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _connection?.Dispose();
        base.Dispose(disposing);
    }
}
