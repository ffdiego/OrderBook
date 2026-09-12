using Core.MatchingEngine;
using Domain.Entities;

namespace Core.ExchangeGateway;

public class ExchangeGatewaySyncLock : ExchangeGatewayBase
{
    private Lock _lock;

    public ExchangeGatewaySyncLock(IMatchingEngine engine) : base(engine)
    {
        _lock = new Lock();
    }

    public override async Task<List<Trade>> ReceiveOrderAsync(Order order)
    {
        lock (_lock)
        {
            return _engine.ProcessOrder(order);
        }
    }
}
