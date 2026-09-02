using Application.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services;

public class ExchangeGateway
{
    private readonly IMatchingEngine _engine;

    public ExchangeGateway(IMatchingEngine engine)
    {
        _engine = engine;
    }

    // ATENÇÃO: Este método será chamado por múltiplas threads simultaneamente.
    // Como vocês vão garantir que o motor cruze as ordens sem corromper o estado em memória?
    public async Task<List<Trade>> ReceiveOrderAsync(Order order)
    {
        return _engine.ProcessOrder(order);
    }

    public static ExchangeGateway DefaultExchangeGateWay() => new(new MatchingEngineSync());
}
