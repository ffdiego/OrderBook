namespace Domain.Enums;

public enum Side { 
    Sell,
    Buy, 
}

public static class SideExtensions
{
    public static Side Opposite(this Side side) => side switch
    {
        Side.Buy => Side.Sell,
        Side.Sell => Side.Buy,
        _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
    };
}
