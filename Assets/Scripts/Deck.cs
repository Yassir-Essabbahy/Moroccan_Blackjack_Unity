using System;
using System.Collections.Generic;

public class Deck
{
    private static readonly Suit[] AllSuits = (Suit[])Enum.GetValues(typeof(Suit));
    private static readonly Rank[] AllRanks = (Rank[])Enum.GetValues(typeof(Rank));

    private readonly List<Card> cards = new List<Card>(40);

    public int RemainingCount => cards.Count;

    public Deck()
    {
        CreateDeck();
        Shuffle();
    }

    private void CreateDeck()
    {
        cards.Clear();

        for (int s = 0; s < AllSuits.Length; s++)
        {
            Suit suit = AllSuits[s];
            for (int r = 0; r < AllRanks.Length; r++)
            {
                cards.Add(new Card(suit, AllRanks[r]));
            }
        }
    }

    public void Shuffle()
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            Card temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

    public Card DrawCard()
    {
        if (cards.Count == 0)
        {
            CreateDeck();
            Shuffle();
        }

        int lastIndex = cards.Count - 1;
        Card drawCard = cards[lastIndex];
        cards.RemoveAt(lastIndex);
        return drawCard;
    }

    public void ResetDeck()
    {
        CreateDeck();
        Shuffle();
    }
}
