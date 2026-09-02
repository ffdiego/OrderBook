using Domain.Entities;

namespace Application.Interfaces
{
    public interface IMatchingEngine
    {
        List<Trade> ProcessOrder(Order order);
        IEnumerable<Order> UnmatchedOrders { get; }
    }

}
