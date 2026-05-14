using EduBridgeMVC.Contracts.Idea;
using EduBridgeMVC.Models;
using Mapster;

namespace EduBridge.Mapping;

public class IdeaCategoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<IdeaCategory, IdeaCategoryResponse>()
            .Map(dest => dest.Tags, src => src.Tags.Select(t => t.Name));
    }
}