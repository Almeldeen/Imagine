using Application.Common.Models;
using Application.Features.AdminDashboard.DTOs;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminDashboard.Queries.GetDashboardStats
{
    public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, BaseResponse<AdminDashboardStatsDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetDashboardStatsQueryHandler(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            UserManager<ApplicationUser> userManager)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userManager = userManager;
        }

        public async Task<BaseResponse<AdminDashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var ordersQuery = _orderRepository.GetAllQueryable();

            var totalOrders = await ordersQuery.CountAsync(cancellationToken);
            var totalRevenue = await ordersQuery
                .Select(o => (decimal?)o.TotalAmount)
                .SumAsync(cancellationToken) ?? 0m;

            var pendingOrders = await ordersQuery
                .Where(o => o.Status == OrderStatus.Pending)
                .CountAsync(cancellationToken);

            var completedOrders = await ordersQuery
                .Where(o => o.Status == OrderStatus.Delivered)
                .CountAsync(cancellationToken);

            var productsQuery = _productRepository.GetAllQueryable();
            var totalProducts = await productsQuery.CountAsync(cancellationToken);

            var clientUsers = await _userManager.GetUsersInRoleAsync("Client");
            var totalCustomers = clientUsers.Count;

            var dto = new AdminDashboardStatsDto
            {
                TotalRevenue = Math.Round(totalRevenue, 2, MidpointRounding.AwayFromZero),
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                CompletedOrders = completedOrders,
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers
            };

            return BaseResponse<AdminDashboardStatsDto>.SuccessResponse(dto, "Dashboard stats retrieved successfully");
        }
    }
}
