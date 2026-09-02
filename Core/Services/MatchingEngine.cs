
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System.Collections.ObjectModel;

namespace Application.Services;

public class MatchingEngine : IMatchingEngine
{
    private readonly List<Order> unmatchedOrders;
    public MatchingEngine()
    {
        unmatchedOrders = new List<Order>();
    }

    public List<Trade> ProcessOrder(Order newOrder)
    {
        List<Trade> trades = [];

        if (unmatchedOrders.Count == 0)
        {
            unmatchedOrders.Add(newOrder);
            return trades;
        }

        IEnumerable<Order> matchingOrders = unmatchedOrders
                .Where(existingOrder => PriceCheck(newOrder, existingOrder))
                .OrderBy(o => o.Price)
                .ThenBy(o => o.Timestamp);

        foreach (Order matchingOrder in matchingOrders) 
        {
            int quantityUsed = Math.Min(newOrder.Quantity, matchingOrder.Quantity);
            newOrder.Quantity -= quantityUsed;
            matchingOrder.Quantity -= quantityUsed;

            Trade trade = new Trade()
            {
                Id = Guid.NewGuid(),
                MakerOrderId = matchingOrder.Id,
                TakerOrderId = newOrder.Id,
                Price = matchingOrder.Price,
                Quantity = quantityUsed
            };
            trades.Add(trade);

            if (matchingOrder.Quantity <= 0)
            {
                unmatchedOrders.Remove(matchingOrder);
            }

            if (newOrder.Quantity <= 0)
            {
                break;
            }
        }

        if (newOrder.Quantity > 0)
        {
            this.unmatchedOrders.Add(newOrder);
        }

        return trades;
    }

    private static bool PriceCheck(Order nova, Order existente) =>
        nova.Side == Side.Buy
            ? nova.Price <= existente.Price
            : nova.Price >= existente.Price;

    public ReadOnlyCollection<Order> Book => unmatchedOrders.AsReadOnly();
}
