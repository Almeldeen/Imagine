using Application.Common.Models;
using Application.Features.Home.DTOs;
using Application.Features.Products.Queries.GetFeaturedProducts;
using Application.Features.Products.Queries.GetLatestProducts;
using Application.Features.Products.Queries.GetPopularProducts;
using Application.Features.Categories.Queries.GetFeaturedCategories;
using MediatR;

namespace Application.Features.Home.Queries.GetHomeData
{
    public class GetHomeDataQueryHandler : IRequestHandler<GetHomeDataQuery, BaseResponse<HomeDataDto>>
    {
        private readonly IMediator _mediator;

        public GetHomeDataQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<BaseResponse<HomeDataDto>> Handle(GetHomeDataQuery request, CancellationToken cancellationToken)
        {
            var featuredProductsTask = _mediator.Send(new GetFeaturedProductsQuery { Take = 8 }, cancellationToken);
            var latestProductsTask = _mediator.Send(new GetLatestProductsQuery { Take = 8 }, cancellationToken);
            var popularProductsTask = _mediator.Send(new GetPopularProductsQuery { Take = 8 }, cancellationToken);
            var featuredCategoriesTask = _mediator.Send(new GetFeaturedCategoriesQuery { Take = 4 }, cancellationToken);

            await Task.WhenAll(featuredProductsTask, latestProductsTask, popularProductsTask, featuredCategoriesTask);

            var hero = new HeroSectionDto
            {
                TitleLine1 = "Create Your",
                TitleLine2 = "Perfect Design",
                Subtitle = "Design custom hoodies and T-shirts with AI assistance. Choose colors, patterns, or upload your own artwork. Your imagination, our technology.",
                BadgeText = "AI-Powered Customization",
                PrimaryCtaText = "Start Customizing",
                SecondaryCtaText = "Watch Demo",
                BannerImageUrl = "/assets/images/hero-banner.png"
            };

            var dto = new HomeDataDto
            {
                Hero = hero,
                FeaturedProducts = featuredProductsTask.Result.Data ?? new(),
                LatestProducts = latestProductsTask.Result.Data ?? new(),
                PopularProducts = popularProductsTask.Result.Data ?? new(),
                FeaturedCategories = featuredCategoriesTask.Result.Data ?? new(),
            };

            return BaseResponse<HomeDataDto>.SuccessResponse(dto, "Home data retrieved successfully");
        }
    }
}
