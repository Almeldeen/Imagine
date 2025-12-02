using Application.Common.Interfaces;
using Application.Common.Mappings;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Seeds;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
                )
            );

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductColorRepository, ProductColorRepository>();
            services.AddScoped<IProductImageRepository, ProductImageRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<ICustomProductRepository, CustomProductRepository>();
            services.AddScoped<ICustomProductColorRepository, CustomProductColorRepository>();
            services.AddScoped<IUserColorSuggestionRepository, UserColorSuggestionRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IWishlistRepository, WishlistRepository>();
            services.AddScoped<IWishlistItemRepository, WishlistItemRepository>();

            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IQueryService, QueryService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Database Seeder
            //services.AddScoped<DatabaseSeeder>();


            // AutoMapper
           




            return services;
        }
    }
}
