using System;
using System.Collections.Generic;

namespace Application.Features.Users.DTOs
{
    public class CustomerListDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CustomerDetailsDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Address { get; set; }
        public int OrdersCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CustomerListResultDto
    {
        public List<CustomerListDto> Items { get; set; } = new List<CustomerListDto>();

        public int TotalAll { get; set; }
        public int TotalActive { get; set; }
        public int TotalInactive { get; set; }
        public int TotalPremium { get; set; }
    }

    public class ImportCustomersResultDto
    {
        public int Imported { get; set; }
        public int Skipped { get; set; }
    }

    public class CreateCustomerDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class UpdateCustomerDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; } = true;
    }
}
