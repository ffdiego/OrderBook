using ChaosTest;
using Core.ExchangeGateway;
using Domain.Entities;
using Xunit;

namespace Tests.ChaosTest;

public abstract class ChaosTestBase
{
    protected abstract ExchangeGateway CreateService();

    [Fact]
    public void DeveProcessarOrdensEmParaleloSemCorromperSaldo()
    {
        ExchangeGateway gateway = CreateService();

        IEnumerable<Order> orders = Utils.Gerar10MilOrdensAleatorias();

        Parallel.ForEach(orders, order =>
        {
            gateway.ReceiveOrderAsync(order).Wait();
        });

        Assert.True(gateway.ValidarIntegridadeDoBook());
    }
}
