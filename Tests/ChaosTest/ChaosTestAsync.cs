using Core.ExchangeGateway;
using Core.MatchingEngine;

namespace Tests.ChaosTest
{
    public class ChaosTestAsync : ChaosTestBase
    {
        protected override ExchangeGatewayBase CreateService() => new ExchangeGatewayAsync(new MatchingEngineAsync());
    }
}
