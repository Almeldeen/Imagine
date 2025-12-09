using Application.Common.Models;
using Application.Features.Users.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Users.Queries.GetCustomers
{
    public class GetCustomersQuery : IRequest<BaseResponse<CustomerListResultDto>>
    {
        // Free text search over full name, email, or phone
        public string? Search { get; set; }

        // Optional role filter (e.g. Admin, Client, or "all")
        public string? Role { get; set; }

        // Status filter: all | active | inactive
        public string? Status { get; set; }

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sort key: name | date | status
        public string? Sort { get; set; }
    }
}
