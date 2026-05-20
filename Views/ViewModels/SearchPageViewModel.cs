namespace EduBridgeMVC.Views.ViewModels;


public record SearchPageViewModel(
    string Query,
    string Type,
    IReadOnlyList<SearchResultViewModel> Results);
