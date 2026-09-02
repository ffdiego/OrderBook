using Application.Interfaces;
using Application.Services;
using ChaosTest;
using Core.Interfaces;
using Domain.Entities;
using Xunit;

namespace ChaosTests;

public class ChaosTests
{
    [Fact]
    public void DeveProcessarOrdensEmParaleloSemCorromperSaldo()
    {
        IMatchingEngine engine = new MatchingEngineSync(); // Sua implementação
        IExchangeGateway gateway = new ExchangeGateway(engine);

        // Simula 10.000 ordens sendo enviadas no mesmo milissegundo
        IEnumerable<Order> orders = Utils.Gerar10MilOrdensAleatorias();

        List<Order> failedOrders = [];

        Parallel.ForEach(orders, order =>
        {
            try
            {
                gateway.ReceiveOrderAsync(order).Wait();
            }
            catch (Exception)
            {
                failedOrders.Add(order);
            }
        });

        // O total de lotes comprados TEM QUE SER IGUAL ao total de lotes vendidos.
        Assert.Multiple(() =>
        {
            Assert.True(engine.ValidarIntegridadeDoBook());
            Assert.Empty(failedOrders);
        });
        
    }
}
