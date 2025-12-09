using Application.Common.Models;
using Application.Features.Users.DTOs;
using Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Queries.GetCustomers
{
    public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, BaseResponse<CustomerListResultDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetCustomersQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<BaseResponse<CustomerListResultDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
        {
            var baseQuery = _userManager.Users.AsNoTracking();

            // Basic search by name, email, or phone
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim().ToLower();

                baseQuery = baseQuery.Where(u =>
                    (u.FirstName + " " + u.LastName).Trim().ToLower().Contains(term) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(term)));
            }

            // Optional role filter (e.g. Client), applied to the base query so that counts respect it
            if (!string.IsNullOrWhiteSpace(request.Role) &&
                !string.Equals(request.Role, "all", StringComparison.OrdinalIgnoreCase))
            {
                var roleUsers = await _userManager.GetUsersInRoleAsync(request.Role.Trim());
                var roleUserIds = roleUsers.Select(u => u.Id).ToHashSet();
                baseQuery = baseQuery.Where(u => roleUserIds.Contains(u.Id));
            }

            // Dynamic counts BEFORE applying the status filter (All/Active/Inactive/Premium)
            var totalAll = await baseQuery.CountAsync(cancellationToken);
            var totalActive = await baseQuery.Where(u => u.IsActive).CountAsync(cancellationToken);
            var totalInactive = await baseQuery.Where(u => !u.IsActive).CountAsync(cancellationToken);
            var totalPremium = await baseQuery.Where(u => u.IsActive && u.Orders.Any()).CountAsync(cancellationToken);

            // Apply status filter (including premium) to the paged query
            var usersQuery = baseQuery;

            if (!string.IsNullOrWhiteSpace(request.Status) &&
                !string.Equals(request.Status, "all", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(request.Status, "premium", StringComparison.OrdinalIgnoreCase))
                {
                    usersQuery = usersQuery.Where(u => u.IsActive && u.Orders.Any());
                }
                else
                {
                    var isActive = string.Equals(request.Status, "active", StringComparison.OrdinalIgnoreCase);
                    usersQuery = usersQuery.Where(u => u.IsActive == isActive);
                }
            }

            // Count after filters (for pagination)
            var totalItems = await usersQuery.CountAsync(cancellationToken);

            // Sorting: simple keys coming from the UI (name | date | status)
            var sortKey = request.Sort?.Trim().ToLowerInvariant();
            usersQuery = sortKey switch
            {
                "name" => usersQuery.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
                "date" => usersQuery.OrderByDescending(u => u.CreatedAt),
                "status" => usersQuery
                    .OrderByDescending(u => u.IsActive)
                    .ThenBy(u => u.FirstName)
                    .ThenBy(u => u.LastName),
                _ => usersQuery.OrderByDescending(u => u.CreatedAt),
            };

            // Pagination
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

            var users = await usersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            // Load roles per user (page-sized, so N is small)
            var result = new List<CustomerListDto>(users.Count);
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var fullName = ($"{user.FirstName} {user.LastName}").Trim();
                var profileImageUrl = string.IsNullOrWhiteSpace(user.ProfileImageUrl)
                    ? "/assets/images/hero-banner.png"
                    : user.ProfileImageUrl;

                result.Add(new CustomerListDto
                {
                    Id = user.Id,
                    FullName = fullName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList(),
                    IsActive = user.IsActive,
                    ProfileImageUrl = profileImageUrl,
                    CreatedAt = user.CreatedAt,
                });
            }

            var wrapper = new CustomerListResultDto
            {
                Items = result,
                TotalAll = totalAll,
                TotalActive = totalActive,
                TotalInactive = totalInactive,
                TotalPremium = totalPremium,
            };

            return BaseResponse<CustomerListResultDto>.SuccessResponse(
                wrapper,
                pageNumber,
                pageSize,
                totalItems,
                "Customers retrieved successfully.");
        }
    }
}
