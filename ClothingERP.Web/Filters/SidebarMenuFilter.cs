using ClothingERP.Application.Interfaces.Repositories;

namespace ClothingERP.Web.Filters;

public class SidebarMenuFilter : IAsyncActionFilter
{
    private readonly IUnitOfWork _uow;

    public SidebarMenuFilter(IUnitOfWork uow) => _uow = uow;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            var roleIdStr = context.HttpContext.User.FindFirst("RoleId")?.Value;
            if (int.TryParse(roleIdStr, out var roleId))
            {
                var permissions = await _uow.RolePermissions.GetByRoleIdAsync(roleId);
                var menus = permissions
                    .Where(p => p.CanView && !p.Module.IsDeleted)
                    .Select(p => p.Module)
                    .OrderBy(m => m.SortOrder)
                    .ToList();

                if (context.Controller is Controller ctrl)
                {
                    ctrl.ViewBag.SidebarMenus = menus;
                    ctrl.ViewBag.CurrentRoleId = roleId;
                    ctrl.ViewBag.CurrentUser = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value;
                    ctrl.ViewBag.CurrentRole = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                }
            }
        }
        await next();
    }
}