using UnityEngine;

public enum Suit
{
    Suit1,
    Suit2,
    Suit3,
    Suit4
}

public enum Rank
{
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Sota = 10,
    Caballo = 11,
    Rey = 12
}

public class Card
{
    public Suit Suit { get; private set; }
    public Rank Rank { get; private set; }
    public int Value { get; private set; }

    public Card(Suit suit, Rank rank)
    {
        Suit = suit;
        Rank = rank;
        Value = CalculateBaseValue(rank);
    }

private int CalculateBaseValue(Rank rank)
{
    if (rank == Rank.One)
        return 11;

    if (rank == Rank.Sota || rank == Rank.Caballo || rank == Rank.Rey)
        return 10;

    return (int)rank;
}
public override string ToString()
{
    return $"{Rank} of {Suit}";
}

}

