using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seeds
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Ensure database is created
                await _context.Database.MigrateAsync();

                // Seed in order of dependencies
                await SeedRolesAsync();
                await SeedAdminUserAsync();
                await SeedCategoriesAsync();
                await SeedProductsAsync();
                await SeedCartsAsync(); // ✅ Cart seeding

                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private async Task SeedRolesAsync()
        {
            var roles = new[] { "Admin", "Client" };

            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole(roleName);
                    var result = await _roleManager.CreateAsync(role);

                    if (result.Succeeded)
                        _logger.LogInformation("Role '{RoleName}' created successfully", roleName);
                    else
                        _logger.LogWarning("Failed to create role '{RoleName}': {Errors}",
                            roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        private async Task SeedAdminUserAsync()
        {
            const string adminEmail = "admin@imagine.com";
            const string adminPassword = "Admin@123456";

            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("Admin user created successfully with email: {Email}", adminEmail);
                }
                else
                {
                    _logger.LogWarning("Failed to create admin user: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        private async Task SeedCategoriesAsync()
        {
            if (await _context.Categories.AnyAsync()) return;

            var categories = new List<Category>
            {
                new() { Name = "Hoodies", Description = "Comfortable and stylish hoodies", ImageUrl ="assets/images/Black Hoodie.png", IsActive = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { Name = "T-Shirts", Description = "Premium quality t-shirts", ImageUrl = "/images/categories/tshirts.jpg", IsActive = true, DisplayOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { Name = "Sweatshirts", Description = "Cozy sweatshirts", ImageUrl = "/images/categories/sweatshirts.jpg", IsActive = true, DisplayOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { Name = "Jackets", Description = "Stylish jackets", ImageUrl = "/images/categories/jackets.jpg", IsActive = true, DisplayOrder = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { Name = "Accessories", Description = "Complete your look", ImageUrl = "/images/categories/accessories.jpg", IsActive = true, DisplayOrder = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} categories", categories.Count);
        }

        private async Task SeedProductsAsync()
        {
            if (await _context.Products.AnyAsync()) return;

            var categories = await _context.Categories.ToListAsync();
            if (!categories.Any())
            {
                _logger.LogWarning("No categories found. Skipping product seeding.");
                return;
            }

            var hoodiesCategory = categories.FirstOrDefault(c => c.Name == "Hoodies");
            var tshirtsCategory = categories.FirstOrDefault(c => c.Name == "T-Shirts");
            var sweatshirtsCategory = categories.FirstOrDefault(c => c.Name == "Sweatshirts");

            var products = new List<Product>();

            if (hoodiesCategory != null)
            {
                products.AddRange(new[]
                {
                    new Product { Name = "Classic Black Hoodie", Description = "Premium cotton blend", BasePrice = 49.99m, CategoryId = hoodiesCategory.Id, IsActive = true, IsFeatured = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { Name = "Gray Pullover Hoodie", Description = "Soft fleece-lined", BasePrice = 54.99m, CategoryId = hoodiesCategory.Id, IsActive = true, IsFeatured = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { Name = "Navy Blue Zip Hoodie", Description = "Full-zip with pockets", BasePrice = 59.99m, CategoryId = hoodiesCategory.Id, IsActive = true, IsFeatured = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                });
            }

            if (tshirtsCategory != null)
            {
                products.AddRange(new[]
                {
                    new Product { Name = "White Cotton T-Shirt", Description = "100% organic cotton", BasePrice = 24.99m, CategoryId = tshirtsCategory.Id, IsActive = true, IsFeatured = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { Name = "Black Graphic T-Shirt", Description = "Modern graphic design", BasePrice = 29.99m, CategoryId = tshirtsCategory.Id, IsActive = true,IsFeatured = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { Name = "Vintage Style T-Shirt", Description = "Retro-inspired design", BasePrice = 27.99m, CategoryId = tshirtsCategory.Id, IsActive = true, IsFeatured = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { Name = "Striped Long Sleeve T-Shirt", Description = "Classic striped pattern", BasePrice = 32.99m, CategoryId = tshirtsCategory.Id, IsActive = true, IsFeatured = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                });
            }

            if (sweatshirtsCategory != null)
            {
                products.AddRange(new[]
                {
                    new Product { Name = "Crew Neck Sweatshirt", Description = "Classic crew neck", BasePrice = 44.99m, CategoryId = sweatshirtsCategory.Id, IsActive = true, IsFeatured = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Product { Name = "Oversized Sweatshirt", Description = "Trendy oversized fit", BasePrice = 49.99m, CategoryId = sweatshirtsCategory.Id, IsActive = true, IsFeatured = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                });
            }

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} products", products.Count);

            // Product colors
            await SeedProductColorsAsync(products);
        }

        private async Task SeedProductColorsAsync(List<Product> products)
        {
            var colors = new List<ProductColor>();
            var colorOptions = new[]
            {
                new { Name = "Black", Hex = "#000000" },
                new { Name = "White", Hex = "#FFFFFF" },
                new { Name = "Navy Blue", Hex = "#000080" },
                new { Name = "Gray", Hex = "#808080" },
                new { Name = "Red", Hex = "#FF0000" },
                new { Name = "Green", Hex = "#008000" }
            };

            foreach (var product in products)
            {
                var productColors = colorOptions
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(Random.Shared.Next(2, 4))
                    .Select(c => new ProductColor
                    {
                        ProductId = product.Id,
                        ColorName = c.Name,
                        ColorHex = c.Hex,
                        Stock = Random.Shared.Next(20, 100),
                        AdditionalPrice = 0m,
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });

                colors.AddRange(productColors);
            }

            await _context.ProductColors.AddRangeAsync(colors);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} product colors", colors.Count);
        }

        private async Task SeedCartsAsync()
        {
            if (await _context.Carts.AnyAsync()) return;

            var productColors = await _context.ProductColors.Take(3).ToListAsync();
            if (!productColors.Any())
            {
                _logger.LogWarning("No product colors found. Skipping cart seeding.");
                return;
            }

            var cart = new Cart
            {
                UserId = null,
                SessionId = "seed-session-001",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            foreach (var color in productColors)
            {
                cart.Items.Add(new CartItem
                {
                    ProductColorId = color.Id,
                    Quantity = 1,
                    UnitPrice = color.Product?.BasePrice ?? 50m,
                    TotalPrice = color.Product?.BasePrice ?? 50m,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded 1 cart with {Count} items successfully", cart.Items.Count);
        }
    }
}
