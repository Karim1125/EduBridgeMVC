using EduBridgeMVC.Contracts.Authentication;
using EduBridgeMVC.Contracts.User;
using EduBridgeMVC.Models;
using Mapster;

namespace EduBridge.Mapping;

public class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ApplicationUser, UserProfileResponse>()
            .Map(dest => dest.Skills,
                src => src.Skills
                    .Where(s => true)
                    .Select(s => new SkillResponse(s.Skill.Id, s.Skill.Name)));

        config.NewConfig<ApplicationUser, UserResponse>()
            .Map(dest => dest.Role, src => string.Empty) // role set manually after mapping
            .Map(dest => dest.IsDisabled, src => src.IsDisabled);

        config.NewConfig<RegisterRequest, ApplicationUser>()
            .Map(dest => dest.UserName, src => src.Email)
            .Map(dest => dest.GitHubUrl, src => src.GitHubUrl)
            .Map(dest => dest.LinkedInUrl, src => src.LinkedInUrl)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.PasswordHash!)
            .Ignore(dest => dest.SecurityStamp!)
            .Ignore(dest => dest.Skills)
            .Ignore(dest => dest.Teams)
            .Ignore(dest => dest.RefreshTokens)
            .Ignore(dest => dest.TeachingAssistant!);

        config.NewConfig<CreateUserRequest, ApplicationUser>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.UserName!)
            .Ignore(dest => dest.PasswordHash!)
            .Ignore(dest => dest.SecurityStamp!)
            .Ignore(dest => dest.Skills)
            .Ignore(dest => dest.Teams)
            .Ignore(dest => dest.RefreshTokens)
            .Ignore(dest => dest.TeachingAssistant!);

        config.NewConfig<UpdateUserRequest, ApplicationUser>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.UserName!)
            .Ignore(dest => dest.Email!)
            .Ignore(dest => dest.PasswordHash!)
            .Ignore(dest => dest.SecurityStamp!)
            .Ignore(dest => dest.Skills)
            .Ignore(dest => dest.Teams)
            .Ignore(dest => dest.RefreshTokens)
            .Ignore(dest => dest.TeachingAssistant!)
            .Ignore(dest => dest.IsDisabled)
            .Ignore(dest => dest.CreatedAt)
            .IgnoreNullValues(true);
    }
}
