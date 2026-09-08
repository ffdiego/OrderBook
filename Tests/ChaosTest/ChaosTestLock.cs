using Core.ExchangeGateway;
using Core.MatchingEngine;

namespace Tests.ChaosTest;

public class ChaosTestSyncLock : ChaosTestBase
{
    protected override ExchangeGateway CreateService()
    {
        IMatchingEngine engine = new MatchingEngineSync();
        return new ExchangeGatewayLock(engine);
    }
}
