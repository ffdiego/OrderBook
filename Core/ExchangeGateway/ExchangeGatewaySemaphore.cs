using Core.MatchingEngine;
using Domain.Entities;

namespace Core.ExchangeGateway
{
    public class ExchangeGatewaySemaphore: ExchangeGateway
    {
        private readonly SemaphoreSlim _asyncSemaphore;

        public ExchangeGatewaySemaphore(IMatchingEngine engine) : base(engine)
        {
            _asyncSemaphore = new SemaphoreSlim(1,1);
        }

        public override async Task<List<Trade>> ReceiveOrderAsync(Order order)
        {
            await _asyncSemaphore.WaitAsync();

            try
            {
                return _engine.ProcessOrder(order);
            }
            finally
            {
                _asyncSemaphore.Release();
            }
        }
    }
}
