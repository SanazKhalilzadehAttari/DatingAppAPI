using AutoMapper;
using DatingAppAPI.DTOs;
using DatingAppAPI.Entities;
using DatingAppAPI.Extnsions;

namespace DatingAppAPI.Helpers
{
    public class AutoMappersProfile:Profile
    {
        public AutoMappersProfile()
        {
            CreateMap<AppUser, MemberDto>()
     .ForMember(
         dest => dest.PhotoUrl,
         opt => opt.MapFrom(src =>
             src.Photos != null && src.Photos.Any(p => p.IsMain) ?
                 src.Photos.First(p => p.IsMain).Url :
                 new Uri("https://upload.wikimedia.org/wikipedia/commons/5/50/User_icon-cp.svg")
         )
     ).ForMember(des => des.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDTO, AppUser>();
            CreateMap<RegisterDTO,AppUser>();
        }
        
    }
}
