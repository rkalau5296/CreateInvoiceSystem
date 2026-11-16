using CreateInvoiceSystem.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class DatabaseModelTests
{
    [Fact]
    public void Model_matches_latest_migration()
    {
        var options = new DbContextOptionsBuilder<CreateInvoiceSystemDbContext>()
            .UseSqlServer("Server=localhost;Database=Unused;Integrated Security=true;TrustServerCertificate=true")
            .Options;

        using var context = new CreateInvoiceSystemDbContext(options);
        Assert.False(context.Database.HasPendingModelChanges());
    }
}
