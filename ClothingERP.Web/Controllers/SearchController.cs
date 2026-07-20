using ClothingERP.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingERP.Web.Controllers;

[Authorize]   
public class SearchController : BaseController
{
    private readonly ISearchService _searchSvc;

    public SearchController(ISearchService searchSvc) => _searchSvc = searchSvc;

    [HttpGet]
    public async Task<IActionResult> Global(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Trim().Length < 2)
            return Json(new List<object>());

        try
        {
            var results = await _searchSvc.GlobalSearchAsync(keyword.Trim(), CurrentUserId);
            return Json(results);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SearchController] Error: {ex.Message}");
            return Json(new List<object>());
        }
    }
}