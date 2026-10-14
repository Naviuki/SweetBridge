using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Infrastructure.Background;
using SweetBridge.Web.Infrastructure.Health;
using SweetBridge.Web.Infrastructure.Persistence;
using SweetBridge.Web.Infrastructure.Security;
using SweetBridge.Web.Infrastructure.Seeding;
using SweetBridge.Web.Options;
using SweetBridge.Web.Services;

const string RussianCulture = "ru-RU";
const string ConnectionStringName = "Default";
const string AdminAreaPath = "/Admin";
const string AdminPolicy = "AdminOnly";
const string UserCookieName = "SweetBridge.User";
const int UserSessionDays = 30;
const int CartSessionMinutes = 60;

CultureInfo russianCulture = CultureInfo.GetCultureInfo(RussianCulture);
CultureInfo.DefaultThreadCurrentCulture = russianCulture;
CultureInfo.DefaultThreadCurrentUICulture = russianCulture;

RequestLocalizationOptions localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(RussianCulture)
    .AddSupportedCultures(RussianCulture)
    .AddSupportedUICultures(RussianCulture);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString(ConnectionStringName)));

BindOptions<LogisticsOptions>(builder, LogisticsOptions.SectionName);
BindOptions<ContactsOptions>(builder, ContactsOptions.SectionName);
BindOptions<EmailOptions>(builder, EmailOptions.SectionName);
BindOptions<SeedOptions>(builder, SeedOptions.SectionName);
BindOptions<ExternalAuthOptions>(builder, ExternalAuthOptions.SectionName);

builder.Services.Configure<RateLimitOptions>(builder.Configuration.GetSection(RateLimitPolicies.OptionsSection));

RateLimitOptions rateLimits = builder.Configuration
    .GetSection(RateLimitPolicies.OptionsSection)
    .Get<RateLimitOptions>() ?? new RateLimitOptions();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(CartSessionMinutes);
    options.Cookie.Name = "SweetBridge.Cart";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddDataProtection()
    .SetApplicationName("SweetBridge")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys")));

builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireDigit = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = UserCookieName;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromDays(UserSessionDays);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AdminPolicy, policy => policy.RequireRole("Admin"));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder(AdminAreaPath, AdminPolicy);
    options.Conventions.AuthorizeFolder("/Account");

    foreach (string anonymousPage in new[]
             {
                 "/Index", "/Catalog", "/Cart", "/Checkout", "/Orders", "/Gifts",
                 "/Info", "/Corporate", "/Newsletter", "/Error",
                 "/Account/Login", "/Account/Register", "/Account/ForgotPassword",
                 "/Account/ResetPassword", "/Account/AccessDenied"
             })
    {
        options.Conventions.AllowAnonymousToPage(anonymousPage);
    }
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "SweetBridge.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(RateLimitPolicies.Global, context => BuildPolicy(context, rateLimits.Global));
    options.AddPolicy(RateLimitPolicies.Auth, context => BuildPolicy(context, rateLimits.Auth));
    options.AddPolicy(RateLimitPolicies.Tracking, context => BuildPolicy(context, rateLimits.Tracking));
    options.AddPolicy(RateLimitPolicies.Promo, context => BuildPolicy(context, rateLimits.Promo));
    options.AddPolicy(RateLimitPolicies.Forms, context => BuildPolicy(context, rateLimits.Forms));
});

builder.Services.AddSweetBridgeServices();
builder.Services.AddScoped<CatalogSeeder>();
builder.Services.AddScoped<WaveSeeder>();
builder.Services.AddScoped<ReferenceDataSeeder>();
builder.Services.AddScoped<DatabaseInitializer>();
builder.Services.AddHostedService<UnpaidOrderCleanupService>();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

WebApplication app = builder.Build();

app.UseForwardedHeaders();
app.UseRequestLocalization(localizationOptions);
app.UseSecurityHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHealthChecks("/health").AllowAnonymous();

await InitializeDatabaseAsync(app);

app.Run();

static void BindOptions<TOptions>(WebApplicationBuilder builder, string sectionName)
    where TOptions : class
{
    builder.Services.AddOptions<TOptions>()
        .Bind(builder.Configuration.GetSection(sectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();
}

static RateLimitPartition<string> BuildPolicy(HttpContext context, RateLimitPolicyOptions policy) =>
    RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = policy.Permits,
            Window = TimeSpan.FromSeconds(policy.WindowSeconds),
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        });

static async Task InitializeDatabaseAsync(WebApplication app)
{
    await using AsyncServiceScope scope = app.Services.CreateAsyncScope();

    DatabaseInitializer initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync(CancellationToken.None);
}