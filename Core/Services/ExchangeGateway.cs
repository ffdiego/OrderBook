using Application.Interfaces;
using Core.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services;

public class ExchangeGateway : IExchangeGateway
{
    private readonly IMatchingEngine _engine;
    private Lock _lock;

    public ExchangeGateway(IMatchingEngine engine)
    {
        _engine = engine;
        _lock = new Lock();
    }

    // ATENÇÃO: Este método será chamado por múltiplas threads simultaneamente.
    // Como vocês vão garantir que o motor cruze as ordens sem corromper o estado em memória?
    public async Task<List<Trade>> ReceiveOrderAsync(Order order)
    {
        lock (_lock)
        {
            return _engine.ProcessOrder(order);
        }
    }
}
