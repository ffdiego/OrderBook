using Domain.Enums;
using System.Drawing;

namespace Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Side Side { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public long Timestamp { get; set; }

    public Order()
    {

    }

    public Order(Side side, int quantity, decimal price, long? ticks = null)
    {
        this.Id = Guid.NewGuid();
        this.Timestamp = ticks ?? DateTime.Now.Ticks;

        this.Side = side;
        this.Price = price;
        this.Quantity = quantity;
    }

    public static Order SellOrder(int quantity, decimal price, long? ticks = null) => new(Side.Sell, quantity, price, ticks);
    public static Order BuyOrder(int quantity, decimal price, long? ticks = null) => new(Side.Buy, quantity, price, ticks);
}
