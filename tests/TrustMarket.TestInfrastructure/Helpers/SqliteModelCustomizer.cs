using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace TrustMarket.TestInfrastructure.Helpers;

// Strips PostgreSQL schemas from the EF Core model so SQLite can create tables.
// Required because SQLite interprets "schema"."table" as an attached-database reference.
public class SqliteModelCustomizer : ModelCustomizer
{
    public SqliteModelCustomizer(ModelCustomizerDependencies dependencies)
        : base(dependencies) { }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        modelBuilder.HasDefaultSchema(null);
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
            entity.SetSchema(null);
    }
}
