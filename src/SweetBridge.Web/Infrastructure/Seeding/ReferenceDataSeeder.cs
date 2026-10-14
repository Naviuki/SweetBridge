using SweetBridge.Web.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Infrastructure.Persistence;
using SweetBridge.Web.Options;
using SweetBridge.Web.Services.Common;

namespace SweetBridge.Web.Infrastructure.Seeding;

/// <summary>
/// Создаёт администратора, пункты выдачи, промокоды и подарочные наборы, если их нет.
/// </summary>
public sealed partial class ReferenceDataSeeder
{
    private const string AdminRole = "Admin";
    private const string AdminPasswordFallback = "ChangeMe!2026";

    private readonly AppDbContext _dbContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SeedOptions _options;
    private readonly IBusinessClock _clock;
    private readonly ILogger<ReferenceDataSeeder> _logger;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="ReferenceDataSeeder"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="userManager">Менеджер пользователей Identity.</param>
    /// <param name="roleManager">Менеджер ролей Identity.</param>
    /// <param name="options">Параметры начального наполнения.</param>
    /// <param name="clock">Источник текущей даты.</param>
    /// <param name="logger">Журнал событий.</param>
    public ReferenceDataSeeder(
        AppDbContext dbContext,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<SeedOptions> options,
        IBusinessClock clock,
        ILogger<ReferenceDataSeeder> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(roleManager);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logger);

        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _options = options.Value;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    /// Заполняет справочные таблицы.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await SeedAdminAsync(cancellationToken);
        await SeedPickupPointsAsync(cancellationToken);
        await SeedPromoCodesAsync(cancellationToken);
        await SeedGiftSetsAsync(cancellationToken);
    }

    private async Task SeedAdminAsync(CancellationToken cancellationToken)
    {
        if (!await _roleManager.RoleExistsAsync(AdminRole))
        {
            IdentityResult roleResult = await _roleManager.CreateAsync(new IdentityRole(AdminRole));

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Не удалось создать роль администратора: {Describe(roleResult)}");
            }
        }

        string email = string.IsNullOrWhiteSpace(_options.AdminEmail)
            ? "admin@sweetbridge.local"
            : _options.AdminEmail.Trim();

        AppUser? admin = await _userManager.FindByEmailAsync(email);
        if (admin is not null)
        {
            return;
        }

        admin = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = "Администратор «Сладкого моста»",
            CreatedAtUtc = _clock.UtcNow
        };

        IdentityResult created = await _userManager.CreateAsync(admin, ResolvePassword());

        if (!created.Succeeded)
        {
            throw new InvalidOperationException($"Не удалось создать администратора: {Describe(created)}");
        }

        IdentityResult assigned = await _userManager.AddToRoleAsync(admin, AdminRole);

        if (!assigned.Succeeded)
        {
            throw new InvalidOperationException($"Не удалось выдать роль администратора: {Describe(assigned)}");
        }

        LogAdminCreated();
    }

    private string ResolvePassword() =>
        string.IsNullOrWhiteSpace(_options.AdminPassword) ? AdminPasswordFallback : _options.AdminPassword;

    private async Task SeedPickupPointsAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.PickupPoints.AnyAsync(cancellationToken))
        {
            return;
        }

        _dbContext.PickupPoints.AddRange(
            PickupPoint.Create(Region.Yakutia, "Якутск", "ПВ-1", "ул. Ленина, 1, ТЦ «А Quaranta», 1 этаж"),
            PickupPoint.Create(Region.Yakutia, "Якутск", "ПВ-2", "ул. Кулаковского, 30, офис Корпорации развития РС(Я)"),
            PickupPoint.Create(Region.Donbass, "Донецк", "ПВ-1", "ул. Артёма, 126, пункт выдачи «Мост»"),
            PickupPoint.Create(Region.Donbass, "Мариуполь", "ПВ-1", "ул. Мира, 42, торговый центр"));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedPromoCodesAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.PromoCodes.AnyAsync(cancellationToken))
        {
            return;
        }

        DateOnly today = _clock.LocalToday;

        _dbContext.PromoCodes.AddRange(
            PromoCode.Create("MOST10", PromoCodeKind.Percent, 10m, today.AddDays(-7), today.AddDays(60), 500, 2000m),
            PromoCode.Create("BRIDGE500", PromoCodeKind.FixedAmount, 500m, today.AddDays(-7), today.AddDays(60), 200, 5000m),
            PromoCode.Create("PEAKIT", PromoCodeKind.Percent, 15m, today, today.AddDays(30), 100, 3000m));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Собирает набор из первых товаров витрины: состав берётся из реального каталога,
    /// поэтому наборы остаются корректными после любого импорта.
    /// </summary>
    /// <param name="products">Товары каталога.</param>
    /// <param name="region">Витрина.</param>
    /// <param name="name">Название набора.</param>
    /// <param name="price">Цена набора.</param>
    /// <returns>Набор или <see langword="null"/>, если в витрине нет товаров.</returns>
    private static GiftSet? BuildGiftSet(IReadOnlyList<Product> products, Region region, string name, decimal price)
    {
        List<Product> showcase = [.. products.Where(product => product.Region == region).Take(3)];

        if (showcase.Count == 0)
        {
            return null;
        }

        string description = string.Join(", ", showcase.Select(product => product.Name.ToLowerInvariant()));

        GiftSet set = GiftSet.Create(name, region, $"Витрина {region.ToDisplayName()}: {description}.", null, price);

        for (var index = 0; index < showcase.Count; index++)
        {
            Product product = showcase[index];
            int quantity = index == 0 ? 2 : 1;

            set.AddItem(GiftSetItem.Create(0, product.Id, product.Name, quantity, product.WeightGrams));
        }

        return set;
    }

    private async Task SeedGiftSetsAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.GiftSets.AnyAsync(cancellationToken))
        {
            return;
        }

        List<Product> products = await _dbContext.Products
            .Where(product => product.IsActive)
            .OrderBy(product => product.Region)
            .ThenBy(product => product.Id)
            .ToListAsync(cancellationToken);

        GiftSet? yakutiaSet = BuildGiftSet(products, Region.Yakutia, "Северный набор", 3490m);
        GiftSet? donbassSet = BuildGiftSet(products, Region.Donbass, "Донбасский набор", 2990m);

        if (yakutiaSet is not null)
        {
            _dbContext.GiftSets.Add(yakutiaSet);
        }

        if (donbassSet is not null)
        {
            _dbContext.GiftSets.Add(donbassSet);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Создан администратор платформы")]
    private partial void LogAdminCreated();

    private static string Describe(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(error => error.Description));
}