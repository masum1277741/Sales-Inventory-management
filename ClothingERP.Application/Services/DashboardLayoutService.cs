using System.Text.Json;
using ClothingERP.Application.Constants;

namespace ClothingERP.Application.Services;

public class DashboardLayoutService : IDashboardLayoutService
{
    private readonly IUnitOfWork _uow;

    public DashboardLayoutService(IUnitOfWork uow) => _uow = uow;

    public async Task<DashboardLayoutDto> GetLayoutAsync(int userId)
    {
        var layout = await _uow.DashboardLayouts.GetQueryable()
            .FirstOrDefaultAsync(l => l.UserId == userId && !l.IsDeleted);

        if (layout == null)
        {
            return new DashboardLayoutDto { Widgets = DashboardWidgetRegistry.GetDefault() };
        }

        try
        {
            var widgets = JsonSerializer.Deserialize<List<WidgetConfigDto>>(layout.LayoutJson)
                          ?? DashboardWidgetRegistry.GetDefault();


            var savedKeys = widgets.Select(w => w.WidgetKey).ToHashSet();
            var maxOrder = widgets.Any() ? widgets.Max(w => w.Order) : 0;
            foreach (var def in DashboardWidgetRegistry.All)
            {
                if (!savedKeys.Contains(def.WidgetKey))
                {
                    widgets.Add(new WidgetConfigDto { WidgetKey = def.WidgetKey, Order = ++maxOrder, IsVisible = false, Size = "Medium" });
                }
            }

            return new DashboardLayoutDto { Widgets = widgets };
        }
        catch
        {
            return new DashboardLayoutDto { Widgets = DashboardWidgetRegistry.GetDefault() };
        }
    }

    // ── Save Layout ────────────────────────────────────────────────────────
    public async Task<ServiceResult> SaveLayoutAsync(int userId, SaveDashboardLayoutDto dto)
    {
        var validKeys = DashboardWidgetRegistry.All.Select(w => w.WidgetKey).ToHashSet();
        var filtered = dto.Widgets.Where(w => validKeys.Contains(w.WidgetKey)).ToList();

        if (!filtered.Any())
            return ServiceResult.Fail("কোনো বৈধ widget পাওয়া যায়নি।");

        var json = JsonSerializer.Serialize(filtered);

        var layout = await _uow.DashboardLayouts.GetQueryable()
            .FirstOrDefaultAsync(l => l.UserId == userId && !l.IsDeleted);

        if (layout == null)
        {
            await _uow.DashboardLayouts.AddAsync(new DashboardLayout
            {
                UserId = userId,
                LayoutJson = json,
                CreatedBy = userId
            });
        }
        else
        {
            layout.LayoutJson = json;
            layout.UpdatedBy = userId;
            layout.UpdatedAt = DateTime.UtcNow;
            _uow.DashboardLayouts.Update(layout);
        }

        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Dashboard layout saved successfully.");
    }

    // ── Reset ─────────────────────────────────────────────────────────────
    public async Task<ServiceResult> ResetToDefaultAsync(int userId)
    {
        var layout = await _uow.DashboardLayouts.GetQueryable()
            .FirstOrDefaultAsync(l => l.UserId == userId && !l.IsDeleted);

        if (layout != null)
        {
            _uow.DashboardLayouts.Remove(layout);
            await _uow.SaveChangesAsync();
        }
        return ServiceResult.Ok("Default layout এ ফিরে যাওয়া হলো।");
    }

    public List<WidgetDefinitionDto> GetAvailableWidgets() => DashboardWidgetRegistry.All;
}