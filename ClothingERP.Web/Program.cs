using ClothingERP.Application;
using ClothingERP.Infrastructure;
using ClothingERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using ClothingERP.Web.Hubs;
using ClothingERP.Web.Realtime;

var builder = WebApplication.CreateBuilder(args);

// ── Services ─────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add<ClothingERP.Web.Filters.SidebarMenuFilter>());
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.Name = "ClothingERP.Auth";
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.MaxAge = null;
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "ClothingERP.Session";
});

builder.Services.AddHttpContextAccessor();

// ── SignalR / Realtime ───────────────────────────────────────────────────
builder.Services.AddSignalR();
builder.Services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();

// ── Pipeline ─────────────────────────────────────────────────────────────
var app = builder.Build();

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbSeeder.SeedAsync(ctx);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStatusCodePagesWithReExecute("/Home/NotFound404");
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapHub<AppHub>("/hubs/app");

app.Run();