using Core.ExchangeGateway;
using Core.MatchingEngine;

namespace Tests.ChaosTest
{
    public class ChaosTestAsync_2 : ChaosTestBase
    {
        protected override ExchangeGatewayBase CreateService() => new ExchangeGatewayAsync(new MatchingEngineAsyncWithSemaphoreVersao2());
    }
}
