using Application;
using Infrastructure;
using Infrastructure.Persistence.Seeds;
using System.Text.Json.Serialization;

namespace Imagine
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Imagine API",
                    Version = "v1",
                    Description = "Imagine eCommerce API - Clean Architecture with CQRS",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "Imagine Team"
                    }
                });

                options.EnableAnnotations();
            });

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Imagine API v1");
                    options.RoutePrefix = string.Empty;
                });
            }

            app.UseHttpsRedirection();

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Seed database
            await app.SeedDatabaseAsync();

            app.Run();
        }
    }
}
