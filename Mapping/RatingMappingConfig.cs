using EduBridgeMVC.Contracts.Rating;
using EduBridgeMVC.Models;
using Mapster;

namespace EduBridge.Mapping;

public class RatingMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Rating, RatingResponse>()
            .Map(dest => dest.TeamName, src => src.Team.Name)
            .Map(dest => dest.TaName, src => $"{src.Ta.User.FirstName} {src.Ta.User.LastName}");
    }
}