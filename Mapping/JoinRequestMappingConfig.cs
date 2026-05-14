using EduBridgeMVC.Contracts.Skills;
using EduBridgeMVC.Models;
using Mapster;

namespace EduBridge.Mapping;

public class JoinRequestMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<JoinRequest, JoinRequestResponse>()
            .Map(dest => dest.TeamName, src => src.Team.Name)
            .Map(dest => dest.StudentName, src => $"{src.Student.FirstName} {src.Student.LastName}")
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);
    }
}