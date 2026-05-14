using EduBridgeMVC.Contracts.Doctor;
using EduBridgeMVC.Models;
using Mapster;

namespace EduBridge.Mapping;

public class DoctorMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Doctor, DoctorResponse>()
            .Map(dest => dest.FullName, src => $"{src.User.FirstName} {src.User.LastName}")
            .Map(dest => dest.Email, src => src.User.Email)
            .Map(dest => dest.ProfileImageUrl, src => src.User.ProfileImageUrl)
            .Map(dest => dest.GitHubUrl, src => src.User.GitHubUrl)
            .Map(dest => dest.LinkedInUrl, src => src.User.LinkedInUrl);

        config.NewConfig<CreateDoctorRequest, Doctor>()
            .Ignore(dest => dest.UserId)
            .Ignore(dest => dest.AvailableTeams);

        config.NewConfig<UpdateDoctorRequest, Doctor>()
            .Ignore(dest => dest.AvailableTeams)
            .Ignore(dest => dest.UserId);
    }
}