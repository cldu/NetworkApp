using AutoMapper;
using Network.API.Dtos;
using Network.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Network.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserListDto>()
            .ForMember(dest => dest.PhotoUrl, opt =>
            {
                opt.MapFrom(src => src.Photos.SingleOrDefault(p => p.IsProfilePhoto).Url);
            })
            .ForMember(dest => dest.Age, opt =>
            {
                opt.ResolveUsing(age => age.DateOfBirth.CalculateAge());
            });

            CreateMap<User, UserDetailsDto>().ForMember(dest => dest.PhotoUrl, opt => {
                opt.MapFrom(src => src.Photos.SingleOrDefault(p => p.IsProfilePhoto).Url);
            })
            .ForMember(dest => dest.Age, opt =>
            {
                opt.ResolveUsing(age => age.DateOfBirth.CalculateAge());
            });

            CreateMap<Photo, PhotoDetailsDto>();
            CreateMap<PhotoCreationDto, Photo>();
            CreateMap<UserUpdateDto, User>();
            CreateMap<UserRegisterDto, User>();
        }
    }
}
