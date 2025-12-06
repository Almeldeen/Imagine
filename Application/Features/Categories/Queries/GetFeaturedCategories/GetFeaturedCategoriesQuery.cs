using Application.Common.Models;
using Application.Features.Categories.DTOs;
using MediatR;

namespace Application.Features.Categories.Queries.GetFeaturedCategories
{
    public class GetFeaturedCategoriesQuery : IRequest<BaseResponse<List<CategoryDto>>>
    {
        public int Take { get; set; } = 4;
    }
}
