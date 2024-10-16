﻿using API.DTOs;
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
    }
}
