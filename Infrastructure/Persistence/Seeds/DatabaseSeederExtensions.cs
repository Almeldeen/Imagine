using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence.Seeds
{
    public static class DatabaseSeederExtensions
    {
        public static async Task SeedDatabaseAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();
        }
    }
}
