using ClothingERP.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace ClothingERP.Web.Services;

public class HttpContextBranchProvider : ICurrentBranchProvider
{
    private readonly IHttpContextAccessor _accessor;
    private const string CookieKey = "clz_current_branch";

    public HttpContextBranchProvider(IHttpContextAccessor accessor) => _accessor = accessor;

    public int GetCurrentBranchId()
    {
        var context = _accessor.HttpContext;
        if (context == null) return 1;  

        var cookieVal = context.Request.Cookies[CookieKey];
        if (int.TryParse(cookieVal, out var branchId) && branchId > 0) return branchId;


        var claim = context.User.FindFirst("DefaultBranchId")?.Value;
        return int.TryParse(claim, out var defaultId) ? defaultId : 1;
    }

    public void SetCurrentBranchId(int branchId)
    {
        var context = _accessor.HttpContext;
        if (context == null) return;

        context.Response.Cookies.Append(CookieKey, branchId.ToString(), new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            HttpOnly = false,  
            SameSite = SameSiteMode.Lax
        });
    }
}