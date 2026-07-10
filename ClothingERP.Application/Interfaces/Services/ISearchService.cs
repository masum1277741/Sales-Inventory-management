namespace ClothingERP.Application.Interfaces.Services;

public interface ISearchService
{
    Task<List<GlobalSearchResultDto>> GlobalSearchAsync(string keyword, int currentUserId);
}