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
    One,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Sota,
    Caballo,
    Rey
}

public class Card
{
    public Suit Suit { get; }
    public Rank Rank { get; }

    public int Value
    {
        get
        {
            switch (Rank)
            {
                case Rank.One:
                    return 11;

                case Rank.Two:
                    return 2;

                case Rank.Three:
                    return 3;

                case Rank.Four:
                    return 4;

                case Rank.Five:
                    return 5;

                case Rank.Six:
                    return 6;

                case Rank.Seven:
                    return 7;

                case Rank.Sota:
                case Rank.Caballo:
                case Rank.Rey:
                    return 10;

                default:
                    return 0;
            }
        }
    }

    public Card(Suit suit, Rank rank)
    {
        Suit = suit;
        Rank = rank;
    }
}
