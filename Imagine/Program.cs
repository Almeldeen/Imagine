using Application;
using Application.Common.Mappings;
using Infrastructure;
using Infrastructure.Persistence.Seeds;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Imagine
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var angularDistPath = Path.Combine(builder.Environment.ContentRootPath,
                "ClientApp", "dist", "imagine.client", "browser");

            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            builder.Services.AddMemoryCache();

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
                    options.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();
            // Serve static files from wwwroot (e.g. /uploads/...)
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                }
            });

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            if (Directory.Exists(angularDistPath))
            {
                var angularFileProvider = new PhysicalFileProvider(angularDistPath);

                app.UseDefaultFiles(new DefaultFilesOptions
                {
                    FileProvider = angularFileProvider
                });

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = angularFileProvider
                });

                app.MapFallback(async context =>
                {
                    var indexFile = Path.Combine(angularDistPath, "index.html");
                    if (!File.Exists(indexFile))
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        return;
                    }

                    context.Response.ContentType = "text/html";
                    await context.Response.SendFileAsync(indexFile);
                });
            }

            // Seed database
            await app.SeedDatabaseAsync();

            app.Run();
        }
    }
}
