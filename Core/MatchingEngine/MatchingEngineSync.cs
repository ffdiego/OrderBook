using Domain.Entities;
using Domain.Enums;

namespace Core.MatchingEngine;

public class MatchingEngineSync : IMatchingEngine
{
    private readonly Dictionary<Side, List<Order>> orders;
    private readonly Dictionary<Side, int> amountNegotiated;

    private readonly List<Trade> trades;
    public MatchingEngineSync()
    {
        orders = new Dictionary<Side, List<Order>>()
        {
            { Side.Buy, []},
            { Side.Sell, []}
        };
        amountNegotiated = new Dictionary<Side, int>()
        {
            { Side.Buy, 0},
            { Side.Sell, 0}
        };

        trades = [];
    }

    public List<Trade> ProcessOrder(Order newOrder)
    {
        List<Trade> trades = [];

        IEnumerable<Order> matchingOrders = orders[newOrder.Side.Opposite()]
                .Where(existingOrder => PriceCheck(newOrder, existingOrder))
                .OrderBy(o => o.Price)
                .ThenBy(o => o.Timestamp);

        orders[newOrder.Side].Add(newOrder);

        foreach (Order matchingOrder in matchingOrders) 
        {
            Trade trade = MakeTrade(newOrder, matchingOrder);
            trades.Add(trade);

            if (newOrder.Quantity <= 0)
            {
                break;
            }

            newOrder.Id = Guid.NewGuid();
        }

        return trades;
    }

    public bool ValidarIntegridadeDoBook()
    {
        if (this.amountNegotiated[Side.Buy] != this.amountNegotiated[Side.Sell])
        {
            throw new Exception($"Compras ({this.amountNegotiated[Side.Buy]}) diferem de vendas ({this.amountNegotiated[Side.Sell]}).");
        }

        int amountTraded = this.trades.Sum(o => o.Quantity);
        if (this.amountNegotiated[Side.Buy] != amountTraded)
        {
            throw new Exception($"Total Negociado ({this.amountNegotiated[Side.Buy]}) difere de Trades ({amountTraded}).");
        }

        return true;
    }

    private void SubtractQuantity(Order order, int quantity)
    {
        order.Quantity -= quantity;
        amountNegotiated[order.Side] += quantity;

        if (order.Quantity <= 0)
        {
            orders[order.Side].Remove(order);
        }
    }

    private Trade MakeTrade(Order newOrder, Order matchingOrder)
    {
        int quantityUsed = Math.Min(newOrder.Quantity, matchingOrder.Quantity);
        SubtractQuantity(matchingOrder, quantityUsed);
        SubtractQuantity(newOrder, quantityUsed);

        Trade trade = new Trade()
        {
            Id = Guid.NewGuid(),
            MakerOrderId = matchingOrder.Id,
            TakerOrderId = newOrder.Id,
            Price = matchingOrder.Price,
            Quantity = quantityUsed
        };

        trades.Add(trade);

        return trade;
    }

    private static bool PriceCheck(Order nova, Order existente) =>
        nova.Side == Side.Buy
            ? nova.Price >= existente.Price
            : nova.Price <= existente.Price;

    public IEnumerable<Order> UnmatchedOrders => orders[Side.Buy].Concat(orders[Side.Sell]).Where(o => o.Quantity > 0);
}
