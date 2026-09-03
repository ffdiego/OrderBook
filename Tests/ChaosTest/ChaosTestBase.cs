using ChaosTest;
using Core.ExchangeGateway;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks; 
using Xunit;

namespace Tests.ChaosTest;

public abstract class ChaosTestBase
{
    protected abstract ExchangeGateway CreateService();

    [Fact]
    public async Task DeveProcessarOrdensEmParaleloSemCorromperSaldo() 
    {
        ExchangeGateway gateway = CreateService();

        IEnumerable<Order> orders = Utils.Gerar10MilOrdensAleatorias();

        await Parallel.ForEachAsync(orders, async (order, cancellationToken) =>
        {
            await gateway.ReceiveOrderAsync(order);
        });

        Assert.True(gateway.ValidarIntegridadeDoBook());
    }
}