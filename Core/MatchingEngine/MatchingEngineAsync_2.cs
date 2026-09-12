using Domain.Entities;
using Domain.Enums;

namespace Core.MatchingEngine
{
    public class MatchingEngineAsync_2 : IMatchingEngineAsync
    {
        private readonly record struct Priority(decimal Price, long Timestamp, long Sequence);

        private static readonly IComparer<Priority> AskComparer = Comparer<Priority>.Create((x, y) =>
        {
            int byPrice = x.Price.CompareTo(y.Price);

            return byPrice != 0 ? byPrice : ByArrival(x, y);

        });

        private static readonly IComparer<Priority> BidComparer = Comparer<Priority>.Create((x, y) =>
        {
            int byPrice = y.Price.CompareTo(x.Price);

            return byPrice != 0 ? byPrice : ByArrival(x, y);

        });

        private static int ByArrival(Priority x, Priority y)
        {
            int byTimeStamp = x.Timestamp.CompareTo(y.Timestamp);
            return byTimeStamp != 0 ? byTimeStamp : x.Sequence.CompareTo(y.Sequence);
        }

        private readonly PriorityQueue<Order, Priority> askHeap;
        private readonly PriorityQueue<Order, Priority> bidHeap;
        private readonly SemaphoreSlim askGate = new(1, 1);
        private readonly SemaphoreSlim bidGate = new(1, 1);

        private readonly Lock bookKeepingGate = new();

        private readonly List<Trade> trades;
        private readonly Dictionary<Side, int> amountNegotiated;
        private long sequence;

        public MatchingEngineAsync_2()
        {
            askHeap = new PriorityQueue<Order, Priority>(AskComparer);
            bidHeap = new PriorityQueue<Order, Priority>(BidComparer);

            amountNegotiated = new Dictionary<Side, int>()
            {
                { Side.Buy, 0},
                { Side.Sell, 0}
            };

            trades = [];
        }
        
        public async Task<List<Trade>> ProcessOrderAsync(Order newOrder)
        {
            Side oppositeSide = newOrder.Side.Opposite();
            List<Trade> executed = [];
            int remaining = newOrder.Quantity;
         
            await askGate.WaitAsync().ConfigureAwait(false);

            try
            {
                await bidGate.WaitAsync().ConfigureAwait(false);

                try
                {
                    remaining = Match(newOrder, remaining, HeapOf(oppositeSide), executed);
                    
                    if (remaining > 0)
                    {
                        Rest(newOrder, remaining);
                    }
                }
                finally
                {
                    bidGate.Release();
                }
            }
            finally
            {
                askGate.Release();
            }
            return executed;
        }

        public List<Trade> ProcessOrder(Order order) => ProcessOrderAsync(order).GetAwaiter().GetResult();

        private int Match(Order taker, int remaining, PriorityQueue<Order, Priority> oppositeHeap, List<Trade> executed)
        {
            while (remaining > 0
                && oppositeHeap.TryPeek(out Order? maker, out Priority makerPriority)
                && PriceCheck(taker, maker))
            {
                oppositeHeap.Dequeue();

                int quantityUsed = Math.Min(remaining, maker.Quantity);
                remaining -= quantityUsed;
                maker.Quantity -= quantityUsed;

                Trade trade = new()
                {
                    Id = Guid.NewGuid(),
                    MakerOrderId = maker.Id,
                    TakerOrderId = taker.Id,
                    Price = maker.Price,
                    Quantity = quantityUsed
                };

                executed.Add(trade);
                Register(trade, taker.Side, maker.Side);

                if(maker.Quantity > 0)
                {
                    oppositeHeap.Enqueue(maker, makerPriority);
                }
            }
            return remaining;
        }

        private void Rest(Order newOrder, int remaining)
        {
            Order resting = new()
            {
                Id = remaining == newOrder.Quantity ? newOrder.Id : Guid.NewGuid(),
                Side = newOrder.Side,
                Price = newOrder.Price,
                Quantity = remaining,
                Timestamp = newOrder.Timestamp
            };

            Priority priority = new(resting.Price, resting.Timestamp, Interlocked.Increment(ref sequence));
            HeapOf(resting.Side).Enqueue(resting, priority);
        }

        private void Register(Trade trade, Side takerSide, Side makerSide)
        {
            lock (bookKeepingGate)
            {
                trades.Add(trade);
                amountNegotiated[takerSide] += trade.Quantity;
                amountNegotiated[makerSide] += trade.Quantity;
            }
        }

        public bool ValidarIntegridadeDoBook()
        {
            askGate.Wait();
            try{
                bidGate.Wait();
                try
                {
                    if (askHeap.TryPeek(out _, out Priority bestAsk)
                        && bidHeap.TryPeek(out _, out Priority bestBid)
                        && bestBid.Price >= bestAsk.Price)
                    {
                        throw new Exception($"Book cruzado: melhor compra ({bestBid.Price}) >= melhor venda ({bestAsk.Price}). ");
                    }
                }
                finally
                {
                    bidGate.Release();
                }
            }
            finally
            {
                askGate.Release();
            }

            lock (bookKeepingGate)
            {
                if (amountNegotiated[Side.Buy] != amountNegotiated[Side.Sell])
                {
                    throw new Exception($"Compras ({amountNegotiated[Side.Buy]}) diferem de vendas ({amountNegotiated[Side.Sell]}).");
                }

                int amountTraded = trades.Sum(trade => trade.Quantity);
                if (amountNegotiated[Side.Buy] != amountTraded)
                {
                    throw new Exception($"Total Negociado ({amountNegotiated[Side.Buy]}) difere de Trades ({amountTraded}).");
                }
            }
            return true;
        }

        public IEnumerable<Order> UnmatchedOrders
        {
            get
            {
                askGate.Wait();
                try
                {
                    bidGate.Wait();
                    try
                    {
                        return askHeap.UnorderedItems
                            .Concat(bidHeap.UnorderedItems)
                            .Select(entry => entry.Element)
                            .Where(order => order.Quantity > 0);
                    }
                    finally
                    {
                        bidGate.Release();
                    }
                }
                finally
                {
                    askGate.Release();
                }
            }
        }

        private PriorityQueue<Order, Priority> HeapOf(Side side) => side == Side.Buy ? bidHeap : askHeap;

        private SemaphoreSlim GateOf(Side side) => side == Side.Buy ? bidGate : askGate;

        private static bool PriceCheck(Order taker, Order maker) =>
            taker.Side == Side.Buy ? taker.Price >= maker.Price
            : taker.Price <= maker.Price;
    }
}
