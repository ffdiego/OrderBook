using Core.ExchangeGateway;
using Core.MatchingEngine;

namespace Tests.ChaosTest;

public class ChaosTestSyncSemaphore : ChaosTestBase
{
    protected override ExchangeGatewayBase CreateService()
    {
        IMatchingEngine engine = new MatchingEngineSync();
        return new ExchangeGatewaySyncSemaphore(engine);
    }
}
