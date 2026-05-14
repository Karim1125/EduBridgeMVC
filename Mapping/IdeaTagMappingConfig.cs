using EduBridgeMVC.Contracts.Idea;
using EduBridgeMVC.Models;
using Mapster;

namespace EduBridge.Mapping;

public class IdeaTagMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<IdeaTag, IdeaTagResponse>()
            .Map(dest => dest.CategoryName, src => src.Category.Name);
    }
}