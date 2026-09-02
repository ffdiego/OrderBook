using Domain.Entities;

namespace Application.Interfaces
{
    public interface IMatchingEngine
    {
        List<Trade> ProcessOrder(Order order);
        bool ValidarIntegridadeDoBook();

        IEnumerable<Order> UnmatchedOrders { get; }
    }

}
