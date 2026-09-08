
using Core.MatchingEngine;
using Domain.Entities;

namespace Core.ExchangeGateway
{
    public class ExchangeGatewayAsync : ExchangeGateway
    {
        private readonly IMatchingEngineAsync _asyncEngine;

        public ExchangeGatewayAsync(IMatchingEngineAsync engine) : base(engine)
        {
            _asyncEngine = engine;
        }

        public override Task<List<Trade>> ReceiveOrderAsync(Order order) => _asyncEngine.ProcessOrderAsync(order);
    }
}
