using Application.Common.Models;
using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Features.Users.Queries.GetCustomerById
{
    public class GetCustomerByIdQuery : IRequest<BaseResponse<CustomerDetailsDto>>
    {
        public string Id { get; set; } = string.Empty;
    }
}
