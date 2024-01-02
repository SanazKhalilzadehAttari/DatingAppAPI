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
            CreateMap<AppUser, MemberDto>().ForMember(
                des => des.PhotoUrl,
                opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p =>p.IsMain).Url))
                .ForMember(des=>des.Age,opt=>opt.MapFrom(src=>src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoDto>();
        }
        
    }
}
