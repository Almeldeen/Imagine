using Application.Common.Models;
using Application.Features.Users.DTOs;
using MediatR;
using System.IO;

namespace Application.Features.Users.Commands.UpdateCustomer
{
    public class UpdateCustomerCommand : IRequest<BaseResponse<bool>>
    {
        public string Id { get; set; } = string.Empty;
        public UpdateCustomerDto Request { get; set; } = new();

        public Stream? ProfileImageStream { get; set; }
        public string? ProfileImageFileName { get; set; }
    }
}
