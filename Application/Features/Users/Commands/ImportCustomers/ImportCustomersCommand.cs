using Application.Common.Models;
using Application.Features.Users.DTOs;
using MediatR;
using System.IO;

namespace Application.Features.Users.Commands.ImportCustomers
{
    public class ImportCustomersCommand : IRequest<BaseResponse<ImportCustomersResultDto>>
    {
        public Stream FileStream { get; set; } = Stream.Null;
        public string? FileName { get; set; }
    }
}
