using Core.ExchangeGateway;
using Core.MatchingEngine;

namespace Tests.ChaosTest
{
    public class ChaosTestAsync : ChaosTestBase
    {
        protected override ExchangeGateway CreateService() => new ExchangeGatewayAsync(new MatchingEngineAsync());
    }
}
