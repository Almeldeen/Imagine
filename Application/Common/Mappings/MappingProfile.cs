using Application.Features.Carts.DTOs;
using AutoMapper;
using Core.Entities;

namespace Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
           CreateMap<AddCartDto, Cart>()
                 .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<AddCartItemDto, CartItem>();

            CreateMap<Cart, CartDto>();
            CreateMap<CartItem, CartItemDto>();
        }
    }
}
