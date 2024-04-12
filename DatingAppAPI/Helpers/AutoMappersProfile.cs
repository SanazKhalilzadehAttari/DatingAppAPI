using AutoMapper;
using DatingAppAPI.DTOs;
using DatingAppAPI.Entities;
using DatingAppAPI.Extnsions;

namespace DatingAppAPI.Helpers
{
    public class AutoMappersProfile : Profile
    {
        public AutoMappersProfile()
        {
            //string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //string imageRelativePath = "...";
            //string imagePath = Path.Combine(baseDirectory, imageRelativePath);
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
            CreateMap<RegisterDTO, AppUser>();

            CreateMap<Message, MessageDto>();
            CreateMap<DateTime, DateTime>().ConvertUsing(d=>DateTime.SpecifyKind(d, DateTimeKind.Utc));
            CreateMap<DateTime?, DateTime?>().ConvertUsing(d => d.HasValue ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc): null);



        }
       /* .ForMember(
                des => des.SenderPhotoUrl,
                opt => opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(p => p.IsMain))
                )
                .ForMember(
                des => des.RecipientPhotoUrl, opt => opt.MapFrom(src =>
                src.Recipient.Photos.FirstOrDefault(p => p.IsMain)));*/

    }

}
