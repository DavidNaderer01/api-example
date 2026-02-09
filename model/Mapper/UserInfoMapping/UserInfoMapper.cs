using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Claims;
using AutoMapper;
using ResponseLibrary.Responses;

namespace Mapper.UserInfoMapping
{
    public class UserInfoMapper : Profile, IUserInfoMapper
    {
        public UserInfoMapper()
        {
            CreateMap<ClaimsPrincipal, UserInfoResponse>()
                .ForMember(dest => dest.Username, 
                    opt => opt.MapFrom(src => src.Identity != null ? src.Identity.Name ?? string.Empty : string.Empty))
                .ForMember(dest => dest.IsAuthenticated, 
                    opt => opt.MapFrom(src => src.Identity != null && src.Identity.IsAuthenticated))
                .ForMember(dest => dest.AuthenticationType, 
                    opt => opt.MapFrom(src => src.Identity != null ? src.Identity.AuthenticationType ?? string.Empty : string.Empty))
                .ForMember(dest => dest.Email, 
                    opt => opt.MapFrom(src => GetClaimValue(src, "email")))
                .ForMember(dest => dest.GivenName, 
                    opt => opt.MapFrom(src => GetClaimValue(src, "given_name")))
                .ForMember(dest => dest.FamilyName, 
                    opt => opt.MapFrom(src => GetClaimValue(src, "family_name")))
                .ForMember(dest => dest.Roles, 
                    opt => opt.MapFrom(src => src.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()))
                .ForMember(dest => dest.Claims, 
                    opt => opt.MapFrom(src => src.Claims.Select(c => new ClaimInfo 
                    { 
                        Type = c.Type, 
                        Value = c.Value 
                    }).ToList()));
        }

        private static string GetClaimValue(ClaimsPrincipal principal, string claimType)
        {
            return principal.FindFirst(claimType)?.Value ?? string.Empty;
        }
    }
}
