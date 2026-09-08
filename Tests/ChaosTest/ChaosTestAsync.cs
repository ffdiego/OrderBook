using Core.ExchangeGateway;
using Core.MatchingEngine;

namespace Tests.ChaosTest
{
    public class ChaosTestAsyncWithSemaphore : ChaosTestBase
    {
        protected override ExchangeGateway CreateService() => new ExchangeGatewayAsync(new MatchingEngineAsyncWithSemaphore());
    }
}
