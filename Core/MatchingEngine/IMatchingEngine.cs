using Domain.Entities;

namespace Core.MatchingEngine
{
    public interface IMatchingEngine
    {
        List<Trade> ProcessOrder(Order order);
        bool ValidarIntegridadeDoBook();

        IEnumerable<Order> UnmatchedOrders { get; }
    }

}
