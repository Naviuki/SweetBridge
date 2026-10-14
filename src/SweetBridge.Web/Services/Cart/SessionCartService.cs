using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Infrastructure.Persistence;
using SweetBridge.Web.Services.Catalog;

namespace SweetBridge.Web.Services.Cart;

/// <summary>
/// Реализация корзины, которая хранит в сессии только идентификаторы товаров и количества,
/// а актуальные цены и веса всегда читает из базы данных.
/// </summary>
public sealed class SessionCartService : ICartService
{
    private const string SessionKey = "SweetBridge.Cart";

    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="SessionCartService"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="httpContextAccessor">Аксессор текущего HTTP-контекста.</param>
    public SessionCartService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(httpContextAccessor);

        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public int GetItemCount() => ReadStates().Sum(state => state.Quantity);

    /// <inheritdoc />
    public int GetQuantity(string productId) =>
        string.IsNullOrWhiteSpace(productId)
            ? 0
            : ReadStates().FirstOrDefault(state =>
                string.Equals(state.ProductId, productId, StringComparison.Ordinal))?.Quantity ?? 0;

    /// <inheritdoc />
    public void Add(string productId, int quantity) =>
        AddItemAsync(productId, quantity, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task<CartView> GetCartAsync(CancellationToken cancellationToken)
    {
        List<CartLineState> states = ReadStates();
        CartView cart = await BuildCartAsync(states, cancellationToken);

        if (cart.Lines.Count != states.Count)
        {
            WriteStates([.. cart.Lines.Select(line => new CartLineState(line.Product.Id, line.Quantity))]);
        }

        return cart;
    }

    /// <inheritdoc />
    public async Task<CartView> AddItemAsync(string productId, int quantity, CancellationToken cancellationToken)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Количество должно быть больше нуля.");
        }

        Product product = await GetRequiredProductAsync(productId, cancellationToken);

        List<CartLineState> states = ReadStates();
        await EnsureSameShowcaseAsync(states, product, cancellationToken);

        CartLineState? existing = states.Find(state => state.ProductId == product.Id);
        int newQuantity = quantity + (existing?.Quantity ?? 0);
        if (newQuantity > CartLimits.MaxQuantityPerProduct)
        {
            throw new DomainException($"Максимальное количество одного товара в заказе — {CartLimits.MaxQuantityPerProduct} шт.");
        }

        states.RemoveAll(state => state.ProductId == product.Id);
        states.Add(new CartLineState(product.Id, newQuantity));
        WriteStates(states);

        return await GetCartAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CartView> SetQuantityAsync(string productId, int quantity, CancellationToken cancellationToken)
    {
        if (quantity > CartLimits.MaxQuantityPerProduct)
        {
            throw new DomainException($"Максимальное количество одного товара в заказе — {CartLimits.MaxQuantityPerProduct} шт.");
        }

        List<CartLineState> states = ReadStates();
        states.RemoveAll(state => state.ProductId == productId);

        if (quantity > 0)
        {
            states.Add(new CartLineState(productId, quantity));
        }

        WriteStates(states);

        return await GetCartAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CartView> RemoveItemAsync(string productId, CancellationToken cancellationToken)
    {
        List<CartLineState> states = ReadStates();
        states.RemoveAll(state => state.ProductId == productId);
        WriteStates(states);

        return await GetCartAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Clear() => Session.Remove(SessionKey);

    private ISession Session =>
        _httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("Корзина доступна только внутри HTTP-запроса.");

    private List<CartLineState> ReadStates()
    {
        string? json = Session.GetString(SessionKey);

        return string.IsNullOrEmpty(json) ? [] : JsonSerializer.Deserialize<List<CartLineState>>(json) ?? [];
    }

    private void WriteStates(List<CartLineState> states)
    {
        if (states.Count == 0)
        {
            Session.Remove(SessionKey);
            return;
        }

        Session.SetString(SessionKey, JsonSerializer.Serialize(states));
    }

    private async Task<CartView> BuildCartAsync(List<CartLineState> states, CancellationToken cancellationToken)
    {
        if (states.Count == 0)
        {
            return new CartView([]);
        }

        string[] ids = [.. states.Select(state => state.ProductId)];

        Dictionary<string, Product> products = await _dbContext.Products
            .AsNoTracking()
            .Where(product => ids.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, StringComparer.Ordinal, cancellationToken);

        List<CartLineView> lines = states
            .Where(state => products.ContainsKey(state.ProductId))
            .Select(state => new CartLineView(ProductSummary.FromProduct(products[state.ProductId]), state.Quantity))
            .ToList();

        return new CartView(lines);
    }

    private async Task<Product> GetRequiredProductAsync(string productId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new DomainException("Не указан товар для добавления в корзину.");
        }

        return await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == productId && product.IsActive, cancellationToken)
            ?? throw new DomainException("Товар не найден в каталоге.");
    }

    private async Task EnsureSameShowcaseAsync(List<CartLineState> states, Product product, CancellationToken cancellationToken)
    {
        if (states.Count == 0)
        {
            return;
        }

        CartView cart = await BuildCartAsync(states, cancellationToken);

        if (!cart.IsEmpty && cart.ShippingDirection != product.ShippingDirection)
        {
            throw new DomainException(
                "В корзине уже есть товары другой витрины. Оформите заказ или очистите корзину, чтобы сменить витрину.");
        }
    }
}
