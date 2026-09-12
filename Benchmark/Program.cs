using System.Globalization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Core.ExchangeGateway;
using Core.MatchingEngine;
using Domain.Entities;
using Domain.Enums;
using Perfolizer.Horology;

IConfig config = ManualConfig.Create(DefaultConfig.Instance)
    .AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance).WithWarmupCount(2).WithIterationCount(5))
    .AddDiagnoser(MemoryDiagnoser.Default)
    .WithSummaryStyle(SummaryStyle.Default.WithTimeUnit(TimeUnit.Millisecond).WithCultureInfo(new CultureInfo("pt-BR")))
    .HideColumns("Error", "Gen0", "Gen1", "Gen2", "Job", "InvocationCount", "IterationCount", "UnrollFactor", "WarmupCount")
    .WithOptions(ConfigOptions.JoinSummary);

BenchmarkRunner.Run([typeof(MatchingEngineSyncTest), typeof(MatchingEngineAsyncTest), typeof(ChaosTest)], config);

public abstract class MatchingEngineTestBase
{
    protected abstract IMatchingEngine CreateEngine();

    [Benchmark]
    public List<Trade> T01AcomodacaoNoBook()
    {
        IMatchingEngine engine = CreateEngine();
        return engine.ProcessOrder(Order.BuyOrder(100, 20));
    }

    [Benchmark]
    public List<Trade> T02OMatchPerfeito()
    {
        IMatchingEngine engine = CreateEngine();
        engine.ProcessOrder(Order.SellOrder(100, 20));
        return engine.ProcessOrder(Order.BuyOrder(100, 20));
    }

    [Benchmark]
    public List<Trade> T03ExecucaoParcial()
    {
        IMatchingEngine engine = CreateEngine();
        engine.ProcessOrder(Order.SellOrder(100, 20));
        List<Trade> trades = engine.ProcessOrder(Order.BuyOrder(150, 20));
        _ = engine.UnmatchedOrders.Count();
        return trades;
    }

    [Benchmark]
    public List<Trade> T04PrioridadePreco()
    {
        IMatchingEngine engine = CreateEngine();
        engine.ProcessOrder(Order.SellOrder(1, 20.50M));
        engine.ProcessOrder(Order.SellOrder(1, 20.00M));
        return engine.ProcessOrder(Order.BuyOrder(1, 21.00M));
    }

    [Benchmark]
    public List<Trade> T05PrioridadeTempo()
    {
        IMatchingEngine engine = CreateEngine();
        engine.ProcessOrder(Order.SellOrder(5, 20.00M, 1000));
        engine.ProcessOrder(Order.SellOrder(5, 20.00M, 1001));
        return engine.ProcessOrder(Order.BuyOrder(5, 20.00M));
    }
}

public class MatchingEngineSyncTest : MatchingEngineTestBase
{
    protected override IMatchingEngine CreateEngine() => new MatchingEngineSync();
}

public class MatchingEngineAsyncTest : MatchingEngineTestBase
{
    protected override IMatchingEngine CreateEngine() => new MatchingEngineAsync();
}

public class ChaosTest
{
    private List<Order> orders = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        Random random = new(20260911);
        orders = Enumerable.Range(0, 10_000).Select(_ => new Order
        {
            Id = Guid.NewGuid(),
            Price = random.Next(1, 11),
            Quantity = random.Next(1, 11),
            Side = (Side)random.Next(0, 2),
            Timestamp = random.Next(1, 2000)
        }).ToList();
    }

    [Benchmark]
    public Task ChaosTestSyncLock() => Run(new ExchangeGatewaySyncLock(new MatchingEngineSync()));

    [Benchmark]
    public Task ChaosTestSyncSemaphore() => Run(new ExchangeGatewaySyncSemaphore(new MatchingEngineSync()));

    [Benchmark]
    public Task ChaosTestAsync() => Run(new ExchangeGatewayAsync(new MatchingEngineAsync()));

    [Benchmark]
    public Task ChaosTestAsync_2() => Run(new ExchangeGatewayAsync(new MatchingEngineAsync_2()));

    private async Task Run(ExchangeGatewayBase gateway)
    {
        List<Order> input = orders
            .Select(o => new Order { Id = o.Id, Price = o.Price, Quantity = o.Quantity, Side = o.Side, Timestamp = o.Timestamp })
            .ToList();

        await Parallel.ForEachAsync(input, async (order, _) => await gateway.ReceiveOrderAsync(order));
        gateway.ValidarIntegridadeDoBook();
    }
}