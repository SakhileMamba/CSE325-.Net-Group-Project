using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace SocialMediaApp.Data;

// Lets EF migration commands create the context without starting the Blazor host.
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var services = new ServiceCollection();
        services.Configure<IdentityOptions>(options =>
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version3);
        var serviceProvider = services.BuildServiceProvider();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=Data/app.db")
            .UseApplicationServiceProvider(serviceProvider)
            .Options;

        return new ApplicationDbContext(options);
    }
}
