using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Martech.Orders.Infrastructure.Persistence;

// Used only by `dotnet ef migrations add` tooling — not referenced at application runtime.
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=martech-orders.db");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
