using EduBridgeMVC.Contracts.TA;
using EduBridgeMVC.Models;
using Mapster;

namespace EduBridge.Mapping;

public class TaMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<TeachingAssistant, TAResponse>()
            .Map(dest => dest.FullName, src => $"{src.User.FirstName} {src.User.LastName}")
            .Map(dest => dest.Email, src => src.User.Email)
            .Map(dest => dest.ProfileImageUrl, src => src.User.ProfileImageUrl)
            .Map(dest => dest.GitHubUrl, src => src.User.GitHubUrl)
            .Map(dest => dest.LinkedInUrl, src => src.User.LinkedInUrl);

        config.NewConfig<CreateTaRequest, TeachingAssistant>()
            .Ignore(dest => dest.UserId)
            .Ignore(dest => dest.AvailableSlots);

        config.NewConfig<UpdateTaRequest, TeachingAssistant>()
            .Ignore(dest => dest.UserId);
    }
}
