namespace ClothingERP.Web.Controllers;

public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard";
        var data = await _dashboard.GetDashboardDataAsync();
        return View(data);
    }
}