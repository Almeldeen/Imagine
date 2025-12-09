using Application.Common.Models;
using Application.Features.Users.DTOs;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Queries.GetCustomerById
{
    public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, BaseResponse<CustomerDetailsDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderRepository _orderRepository;

        public GetCustomerByIdQueryHandler(UserManager<ApplicationUser> userManager, IOrderRepository orderRepository)
        {
            _userManager = userManager;
            _orderRepository = orderRepository;
        }

        public async Task<BaseResponse<CustomerDetailsDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                return BaseResponse<CustomerDetailsDto>.FailureResponse("A valid customer id is required.");
            }

            var user = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (user == null)
            {
                return BaseResponse<CustomerDetailsDto>.FailureResponse("Customer not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var ordersCount = await _orderRepository
                .GetAllQueryable()
                .AsNoTracking()
                .CountAsync(o => o.UserId == user.Id, cancellationToken);

            var fullName = ($"{user.FirstName} {user.LastName}").Trim();
            var profileImageUrl = string.IsNullOrWhiteSpace(user.ProfileImageUrl)
                ? "/assets/images/hero-banner.png"
                : user.ProfileImageUrl;

            var dto = new CustomerDetailsDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = fullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                ProfileImageUrl = profileImageUrl,
                Address = user.Address,
                OrdersCount = ordersCount,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
            };

            return BaseResponse<CustomerDetailsDto>.SuccessResponse(dto, "Customer retrieved successfully.");
        }
    }
}
