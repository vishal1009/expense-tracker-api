using AutoMapper;
using ExpenseTrackerApi.DTOs;
using ExpenseTrackerApi.Models;

namespace ExpenseTrackerApi.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CategoryDto, Category>();
            CreateMap<Category, CategoryResponseDto>();
            CreateMap<ExpenseRequestDto, Expense>();
            CreateMap<Expense, ExpenseResponseDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category!.Name));
        }
    }
}
