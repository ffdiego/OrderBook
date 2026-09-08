using Core.MatchingEngine;

namespace Tests.MatchingEngineTest
{
    public class MatchingEngineAsyncTest : MatchingEngineTestBase
    {
        protected override IMatchingEngine CreateEngine() => new MatchingEngineAsyncWithSemaphore();
    }
}
