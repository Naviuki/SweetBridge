using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using SweetWave.Web.Infrastructure.Persistence;
using SweetWave.Web.Infrastructure.Seeding;
using SweetWave.Web.Options;
using SweetWave.Web.Services;

const string RussianCulture = "ru-RU";
const string ConnectionStringName = "DefaultConnection";
const string AdminAreaPath = "/Admin";
const string AdminLoginPath = "/Admin/Login";
const string AdminCookieName = "SweetWave.Admin";
const int AdminSessionHours = 8;
const int CartSessionMinutes = 30;

CultureInfo russianCultureInfo = CultureInfo.GetCultureInfo(RussianCulture);
CultureInfo.DefaultThreadCurrentCulture = russianCultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = russianCultureInfo;

RequestLocalizationOptions localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(RussianCulture)
    .AddSupportedCultures(RussianCulture)
    .AddSupportedUICultures(RussianCulture);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString(ConnectionStringName)));

builder.Services.AddOptions<LogisticsOptions>()
    .Bind(builder.Configuration.GetSection(LogisticsOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<AdminOptions>()
    .Bind(builder.Configuration.GetSection(AdminOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(CartSessionMinutes);
    options.Cookie.Name = "SweetWave.Cart";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddRazorPages(options => options.Conventions.AuthorizeFolder(AdminAreaPath));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = AdminLoginPath;
        options.AccessDeniedPath = AdminLoginPath;
        options.Cookie.Name = AdminCookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(AdminSessionHours);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddSweetWaveServices();
builder.Services.AddScoped<CatalogSeeder>();
builder.Services.AddScoped<WaveSeeder>();
builder.Services.AddScoped<DatabaseInitializer>();

WebApplication app = builder.Build();

app.UseRequestLocalization(localizationOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

await InitializeDatabaseAsync(app);

app.Run();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    await using AsyncServiceScope scope = app.Services.CreateAsyncScope();

    DatabaseInitializer initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync(CancellationToken.None);
}