using Core.MatchingEngine;
using Domain.Entities;

namespace Core.ExchangeGateway;

public abstract class ExchangeGatewayBase
{
    protected readonly IMatchingEngine _engine;

    public ExchangeGatewayBase(IMatchingEngine engine)
    {
        _engine = engine;
    }

    // ATENÇÃO: Este método será chamado por múltiplas threads simultaneamente.
    // Como vocês vão garantir que o motor cruze as ordens sem corromper o estado em memória?
    public virtual async Task<List<Trade>> ReceiveOrderAsync(Order order)
    {
        return _engine.ProcessOrder(order);
    }

    public bool ValidarIntegridadeDoBook() => _engine.ValidarIntegridadeDoBook();
}