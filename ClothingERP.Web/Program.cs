using ClothingERP.Application;
using ClothingERP.Application.Interfaces.Services;
using ClothingERP.Infrastructure;
using ClothingERP.Infrastructure.Data;
using ClothingERP.Infrastructure.PaymentGateways;
using ClothingERP.Web.BackgroundServices;
using ClothingERP.Web.Hubs;
using ClothingERP.Web.Realtime;
using ClothingERP.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

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
// ── HttpClient for Exchange Rate API ──────────────────────────────────────
builder.Services.AddHttpClient("ExchangeRateApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ── HttpClient for bKash / Nagad Payment Gateways ──────────────────────────
builder.Services.AddHttpClient<BkashApiClient>();
builder.Services.AddHttpClient<NagadApiClient>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
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
    })
    .AddCookie("CustomerAuth", options =>
    {
        options.Cookie.Name = ".ClozeyShop.Auth";
        options.LoginPath = "/Shop/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "ClothingERP.Session";
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentBranchProvider, HttpContextBranchProvider>();

// ── SignalR / Realtime ───────────────────────────────────────────────────
builder.Services.AddSignalR();
builder.Services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();
builder.Services.AddHostedService<ExchangeRateBackgroundService>();
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
    name: "shop",
    pattern: "Shop/{action=Index}/{id?}",
    defaults: new { controller = "Shop" });

app.MapControllerRoute(
    name: "shopaccount",
    pattern: "ShopAccount/{action=Login}/{id?}",
    defaults: new { controller = "ShopAccount" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapHub<AppHub>("/hubs/app");

app.Run();