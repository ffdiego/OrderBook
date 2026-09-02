using Core.MatchingEngine;

namespace Tests.MatchingEngineTest;

public class MatchingEngineSyncTest : MatchingEngineTestBase
{
    protected override IMatchingEngine CreateEngine()
    {
        return new MatchingEngineSync();
    }
}
