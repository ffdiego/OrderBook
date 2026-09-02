using Application.Interfaces;
using Application.Services;
using ChaosTest;
using Domain.Entities;
using Xunit;

namespace ChaosTests;

public class ChaosTests
{
    [Fact]
    public void DeveProcessarOrdensEmParaleloSemCorromperSaldo()
    {
        IMatchingEngine engine = new MatchingEngineSync(); // Sua implementação
        ExchangeGateway gateway = new ExchangeGateway(engine);

        // Simula 10.000 ordens sendo enviadas no mesmo milissegundo
        IEnumerable<Order> orders = Utils.Gerar10MilOrdensAleatorias();

        Parallel.ForEach(orders, order =>
        {
            gateway.ReceiveOrderAsync(order).Wait();
        });

        // O total de lotes comprados TEM QUE SER IGUAL ao total de lotes vendidos.
        //Assert.True(engine.ValidarIntegridadeDoBook());
    }
}
