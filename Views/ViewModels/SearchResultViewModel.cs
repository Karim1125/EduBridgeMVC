namespace EduBridgeMVC.Views.ViewModels;


public record SearchResultViewModel(
    string Id,
    string Name,
    string Type,
    string Email,
    string? Department,
    string? ProfileImageUrl,
    string ControllerName);
