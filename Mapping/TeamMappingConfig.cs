using EduBridgeMVC.Contracts.Team;
using EduBridgeMVC.Models;
using Mapster;

namespace EduBridge.Mapping;

public class TeamMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Team, TeamResponse>()
            .Map(dest => dest.LeaderName, src => $"{src.Leader.FirstName} {src.Leader.LastName}")
            .Map(dest => dest.CurrentMembers, src => src.Members.Count)
            .Map(dest => dest.TaName, src => src.Ta != null ? $"{src.Ta.User.FirstName} {src.Ta.User.LastName}" : null)
            .Map(dest => dest.DoctorName,
                src => src.Doctor != null ? $"{src.Doctor.User.FirstName} {src.Doctor.User.LastName}" : null);
    }
}