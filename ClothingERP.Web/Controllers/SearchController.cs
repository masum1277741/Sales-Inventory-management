namespace ClothingERP.Web.Controllers;

public class SearchController : BaseController
{
    private readonly ISearchService _searchSvc;

    public SearchController(ISearchService searchSvc) => _searchSvc = searchSvc;

    // ── AJAX: Global Search ──────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Global(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Trim().Length < 2)
            return Json(new List<GlobalSearchResultDto>());

        var results = await _searchSvc.GlobalSearchAsync(keyword, CurrentUserId);
        return Json(results);
    }
}