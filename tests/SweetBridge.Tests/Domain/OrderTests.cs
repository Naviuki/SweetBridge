using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Tests.Domain;

/// <summary>
/// Тесты жизненного цикла статусов <see cref="Order"/>.
/// </summary>
public sealed class OrderTests
{
    private static readonly DateTime CreatedAt = new(2025, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ChangedAt = new(2025, 3, 5, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_NewOrder_IsAcceptedWithSingleHistoryEntry()
    {
        Order order = CreateOrder();

        Assert.Equal(OrderStatus.Accepted, order.Status);
        Assert.Single(order.History);
        Assert.Equal(OrderStatus.Accepted, order.History[0].Status);
        Assert.Equal(CreatedAt, order.History[0].ChangedAtUtc);
    }

    [Fact]
    public void Create_PersonalOrder_CopiesBuyerIntoRecipient()
    {
        Order order = CreateOrder();

        Assert.False(order.IsGift);
        Assert.Equal("Пётр", order.RecipientName);
        Assert.Equal("+79990001122", order.RecipientPhone);
        Assert.Null(order.GiftMessage);
    }

    [Fact]
    public void Create_GiftWithoutRecipientName_ThrowsDomainException()
    {
        DomainException exception = Assert.Throws<DomainException>(() => Order.Create(
            "SW-000001",
            null,
            CreatedAt,
            "Пётр",
            "+79990001122",
            isGift: true,
            recipientName: null,
            recipientPhone: "+79990002233",
            recipientCity: "Якутск",
            pickupPointId: 1,
            recipientPickupPoint: "ПВЦ",
            giftMessage: "С днём рождения!",
            waveId: 1));

        Assert.Contains("получателя", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AdvanceStatus_FromAccepted_MovesToAssemblingAndRecordsHistory()
    {
        Order order = CreateOrder();

        order.AdvanceStatus(ChangedAt);

        Assert.Equal(OrderStatus.Assembling, order.Status);
        Assert.Equal(2, order.History.Count);
        Assert.Equal(OrderStatus.Assembling, order.History[1].Status);
        Assert.Equal(ChangedAt, order.History[1].ChangedAtUtc);
    }

    [Fact]
    public void AdvanceStatus_ThroughWholeChain_ReachesReceivedAndKeepsFullTimeline()
    {
        Order order = CreateOrder();

        OrderStatus[] expectedChain =
        [
            OrderStatus.Assembling,
            OrderStatus.InTransit,
            OrderStatus.ReadyForPickup,
            OrderStatus.Received
        ];

        foreach (OrderStatus expected in expectedChain)
        {
            order.AdvanceStatus(ChangedAt);

            Assert.Equal(expected, order.Status);
        }

        Assert.Equal(5, order.History.Count);
        Assert.Equal(OrderStatus.Accepted, order.History[0].Status);
        Assert.Equal(OrderStatus.Received, order.History[4].Status);
    }

    [Fact]
    public void AdvanceStatus_FromReceived_ThrowsDomainException()
    {
        Order order = CreateOrder();

        MoveToReceived(order);

        DomainException exception = Assert.Throws<DomainException>(() => order.AdvanceStatus(ChangedAt));

        Assert.Equal(OrderStatus.Received, order.Status);
        Assert.Contains("уже получен", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Ship_FromAccepted_MovesToInTransit()
    {
        Order order = CreateOrder();

        order.Ship(ChangedAt);

        Assert.Equal(OrderStatus.InTransit, order.Status);
        Assert.Equal(OrderStatus.InTransit, order.History[^1].Status);
    }

    [Fact]
    public void Ship_FromAssembling_MovesToInTransit()
    {
        Order order = CreateOrder();
        order.AdvanceStatus(ChangedAt);

        order.Ship(ChangedAt);

        Assert.Equal(OrderStatus.InTransit, order.Status);
    }

    [Fact]
    public void Ship_OrderAlreadyInTransit_ThrowsDomainException()
    {
        Order order = CreateOrder();
        order.Ship(ChangedAt);

        Assert.Throws<DomainException>(() => order.Ship(ChangedAt));
        Assert.Equal(OrderStatus.InTransit, order.Status);
    }

    [Fact]
    public void AddItem_SameProductTwice_ThrowsDomainException()
    {
        Order order = CreateOrder();
        order.AddItem(CreateItem());

        Assert.Throws<DomainException>(() => order.AddItem(CreateItem()));
    }

    [Fact]
    public void AddItems_CalculatesWeightAndGoodsCost()
    {
        Order order = CreateOrder();

        order.AddItem(CreateItem());
        order.AddItem(OrderItem.Create("DNB-002", "Пряник", 100m, 250, 2));
        order.ApplyCosts(800m, 150m, 20m, 0m, null);

        Assert.Equal(1500, order.TotalWeightGrams);
        Assert.Equal(800m, order.GoodsCost);
        Assert.Equal(800m, order.ShippingCost);
        Assert.Equal(150m, order.ThermoPackCost);
        Assert.Equal(20m, order.FillDiscountPercent);
        Assert.Equal(1750m, order.Total);
    }

    [Fact]
    public void ApplyShippingCosts_DiscountOutOfRange_ThrowsDomainException()
    {
        Order order = CreateOrder();
        order.AddItem(CreateItem());

        Assert.Throws<DomainException>(() => order.ApplyCosts(800m, 0m, 120m, 0m, null));
    }

    private static Order CreateOrder() => Order.Create(
        "SW-000001",
        null,
        CreatedAt,
        "Пётр",
        "+79990001122",
        isGift: false,
        recipientName: null,
        recipientPhone: null,
        recipientCity: "Якутск",
        pickupPointId: 1,
        recipientPickupPoint: "ПВЦ",
        giftMessage: null,
        waveId: 1);

    private static OrderItem CreateItem() => OrderItem.Create("YKT-001", "Шоколад", 300m, 500, 2);

    private static void MoveToReceived(Order order)
    {
        for (var step = 0; step < 4; step++)
        {
            order.AdvanceStatus(ChangedAt);
        }
    }
}
