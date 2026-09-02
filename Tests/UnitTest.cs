using Application.Services;
using Domain.Entities;
using System.Collections.ObjectModel;
using Xunit;

namespace UnitTests;

public class UnitTest
{
    [Fact]
    public void T01AcomodacaoNoBook()
    {
        // Arrange
        MatchingEngine matchingEngine = new MatchingEngine();

        // Act
        Order order = Order.BuyOrder(100, 20);
        List<Trade> trades = matchingEngine.ProcessOrder(order);

        // Assert
        Assert.Empty(trades);
    }

    [Fact]
    public void T02OMatchPerfeito()
    {
        // Arrange
        MatchingEngine matchingEngine = new MatchingEngine();
        Order sellOrder = Order.SellOrder(100, 20);
        matchingEngine.ProcessOrder(sellOrder);

        // Act
        Order buyOrder = Order.BuyOrder(100, 20);
        List<Trade> trades = matchingEngine.ProcessOrder(buyOrder);

        // Assert
        var trade = Assert.Single(trades);
        Assert.Equal(100, trade.Quantity);
        Assert.Equal(20, trade.Price);
    }

    [Fact]
    public void T03ExecucaoParcial()
    {
        // Arrange
        MatchingEngine matchingEngine = new MatchingEngine();
        Order sellOrder = Order.SellOrder(100, 20);
        matchingEngine.ProcessOrder(sellOrder);

        // Act
        Order buyOrder = Order.BuyOrder(150, 20);
        List<Trade> trades = matchingEngine.ProcessOrder(buyOrder);

        // Assert
        var trade = Assert.Single(trades);
        Assert.Equal(100, trade.Quantity);
        Assert.Equal(20, trades[0].Price);

        var unmatchedOrder = Assert.Single(matchingEngine.Book);
        Assert.Equal(50, unmatchedOrder.Quantity);
        Assert.Equal(20, unmatchedOrder.Price);
        Assert.NotEqual(buyOrder.Id, unmatchedOrder.Id);
    }

    [Fact]
    public void T04PrioridadePreco()
    {
        // Arrange
        MatchingEngine matchingEngine = new MatchingEngine();
        Order sellOrderA = Order.SellOrder(1, 20.50M);
        Order sellOrderB = Order.SellOrder(1, 20.00M);
        matchingEngine.ProcessOrder(sellOrderA);
        matchingEngine.ProcessOrder(sellOrderB);

        // Act
        Order buyOrder = Order.BuyOrder(1, 21.00M);
        List<Trade> trades = matchingEngine.ProcessOrder(buyOrder);

        // Assert
        var trade = Assert.Single(trades);
        Assert.Equal(1, trade.Quantity);
        Assert.Equal(20, trade.Price);
    }

    [Fact]
    public void T05PrioridadeTempo()
    {
        // Arrange
        MatchingEngine matchingEngine = new MatchingEngine();
        Order sellOrderA = Order.SellOrder(5, 20.00M, 1000);
        Order sellOrderB = Order.SellOrder(5, 20.00M, 1001);
        matchingEngine.ProcessOrder(sellOrderA);
        matchingEngine.ProcessOrder(sellOrderB);

        // Act
        Order buyOrder = Order.BuyOrder(5, 20.00M);
        List<Trade> trades = matchingEngine.ProcessOrder(buyOrder);
        ReadOnlyCollection<Order> book = matchingEngine.Book;

        // Assert
        var trade = Assert.Single(trades);
        Assert.Equal(5, trade.Quantity);
        Assert.Equal(20, trade.Price);
        Assert.Equal(sellOrderA.Id, trade.MakerOrderId);
    }
}
