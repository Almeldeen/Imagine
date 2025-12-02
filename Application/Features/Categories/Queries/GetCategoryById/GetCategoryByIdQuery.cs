using Application.Common.Models;
using Application.Features.Categories.DTOs;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategoryById
{
    public class GetCategoryByIdQuery : IRequest<BaseResponse<CategoryDto>>
    {
        public int Id { get; set; }
    }
}
