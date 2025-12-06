using System.Collections.Generic;
using Application.Features.Categories.DTOs;
using Application.Features.Products.DTOs;

namespace Application.Features.Home.DTOs
{
    public class HeroSectionDto
    {
        public string TitleLine1 { get; set; } = string.Empty;
        public string TitleLine2 { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string BadgeText { get; set; } = string.Empty;
        public string PrimaryCtaText { get; set; } = string.Empty;
        public string SecondaryCtaText { get; set; } = string.Empty;
        public string BannerImageUrl { get; set; } = string.Empty;
    }

    public class HomeDataDto
    {
        public HeroSectionDto Hero { get; set; } = new HeroSectionDto();
        public List<ProductListDto> FeaturedProducts { get; set; } = new();
        public List<ProductListDto> LatestProducts { get; set; } = new();
        public List<ProductListDto> PopularProducts { get; set; } = new();
        public List<CategoryDto> FeaturedCategories { get; set; } = new();
    }
}
