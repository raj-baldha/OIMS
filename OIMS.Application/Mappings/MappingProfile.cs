using AutoMapper;
using OIMS.Application.DTOs.Products;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OIMS.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, LoginResponseDto>().ReverseMap();
            CreateMap<CreatedCategoryResponseDto, Category>().ReverseMap();
            CreateMap<CustomerLoginResponseDto, Customer>().ReverseMap();
        }
    }
}
