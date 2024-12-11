using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDto>()
            .ForMember(destination => destination.Age,
                       option => option.MapFrom(source => source.DateOfBirth.CalculateAge()))
            .ForMember(destination => destination.PhotoUrl, 
                       options => options.MapFrom(source => source.Photos.FirstOrDefault(x => x.IsMain)!.Url));

        CreateMap<Photo, PhotoDto>();

        CreateMap<RegisterDto, AppUser>();

        CreateMap<MemberUpdateDto, AppUser>();

        CreateMap<string, DateOnly>().ConstructUsing(source => DateOnly.Parse(source));

        CreateMap<Message, MessageDto>()
            .ForMember(d => d.SenderPhotoUrl, o => o.MapFrom(s => s.Sender.Photos.FirstOrDefault(x => x.IsMain)!.Url))
            .ForMember(d => d.RecipientPhotoUrl, o => o.MapFrom(s => s.Recipient.Photos.FirstOrDefault(x => x.IsMain)!.Url));

        CreateMap<DateTime, DateTime>().ConvertUsing(dt => DateTime.SpecifyKind(dt, DateTimeKind.Utc));
        CreateMap<DateTime?, DateTime?>().ConvertUsing(dt => dt.HasValue ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc) : null);

        CreateMap<Photo, PhotoForModerationDto>().
            ForMember(d => d.UserName, o => o.MapFrom(s => s.AppUser.UserName));
    }
}
