using Domain.Entities;

namespace Core.MatchingEngine
{
    public interface IMatchingEngineAsync : IMatchingEngine
    {
        Task<List<Trade>> ProcessOrderAsync(Order order);
    }
}
