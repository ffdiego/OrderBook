using Core.ExchangeGateway;
using Core.MatchingEngine;

namespace Tests.ChaosTest;

public class ChaosTestLockAsyncEngine : ChaosTestBase
{
    protected override ExchangeGateway CreateService()
    {
        IMatchingEngine engine = new MatchingEngineAsync();
        return new ExchangeGatewayLock(engine);
    }
}
