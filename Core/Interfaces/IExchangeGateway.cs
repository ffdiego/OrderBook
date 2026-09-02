using Domain.Entities;

namespace Core.Interfaces;

public interface IExchangeGateway
{
    Task<List<Trade>> ReceiveOrderAsync(Order order);
}