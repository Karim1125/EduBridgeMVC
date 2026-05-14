using EduBridgeMVC.Contracts.TA;
using EduBridgeMVC.Models;
using Mapster;

namespace EduBridge.Mapping;

public class TaRequestMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<TaRequest, TaRequestResponse>()
            .Map(dest => dest.TeamName, src => src.Team.Name)
            .Map(dest => dest.TaName, src => $"{src.TA.User.FirstName} {src.TA.User.LastName}")
            .Map(dest => dest.Department, src => src.TA.Department)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);
    }
}