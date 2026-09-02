namespace Domain.Enums;

public enum Side { 
    Sell,
    Buy, 
}

public static class SideExtensions
{
    extension(Side side)
    {
        public Side Opposite => side switch
        {
            Side.Buy => Side.Sell,
            Side.Sell => Side.Buy,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
        };
    }
}
