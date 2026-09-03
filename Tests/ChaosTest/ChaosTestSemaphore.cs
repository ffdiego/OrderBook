using Core.ExchangeGateway;
using Core.MatchingEngine;

namespace Tests.ChaosTest;

public class ChaosTestSemaphore : ChaosTestBase
{
    protected override ExchangeGateway CreateService()
    {
        IMatchingEngine engine = new MatchingEngineSync();
        return new ExchangeGatewaySemaphore(engine);
    }
}
