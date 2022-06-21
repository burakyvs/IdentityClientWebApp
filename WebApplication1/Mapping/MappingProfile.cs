using AutoMapper;
using WebApplication1.Models;

namespace WebApplication1.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AuthResponse, object>().ReverseMap();
        }
    }
}
