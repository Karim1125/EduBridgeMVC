using EduBridgeMVC.Contracts.Doctor;
using EduBridgeMVC.Models;
using Mapster;

namespace EduBridge.Mapping;

public class DoctorRequestMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DoctorRequest, DoctorRequestResponse>()
            .Map(dest => dest.TeamName, src => src.Team.Name)
            .Map(dest => dest.DoctorName, src => $"{src.Doctor.User.FirstName} {src.Doctor.User.LastName}")
            .Map(dest => dest.Status, src => src.Status.ToString());
    }
}