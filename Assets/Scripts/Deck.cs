using System;
using System.Collections.Generic;

public class Deck
{
    private readonly List<Card> cards = new List<Card>();

    public int RemainingCount => cards.Count;

    public Deck()
    {
        CreateDeck();
        Shuffle();
    }

    private void CreateDeck()
    {
        cards.Clear();

        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                cards.Add(new Card(suit, rank));
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
