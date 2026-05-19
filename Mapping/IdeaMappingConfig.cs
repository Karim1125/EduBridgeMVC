using EduBridgeMVC.Contracts.Idea;
using EduBridgeMVC.Models;
using Mapster;

namespace EduBridge.Mapping;

public class IdeaMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Idea, IdeaResponse>()
            .Map(dest => dest.CategoryName, src => src.Category.Name)
            .Map(dest => dest.Tags, src => src.Tags.Select(t => new IdeaTagDto(t.Id, t.Name)))
            .Map(dest => dest.TeamName, src => src.Team.Name);
    }
}
