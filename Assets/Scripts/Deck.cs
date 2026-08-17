using UnityEngine;
using System.Collections.Generic;
using System;

public class Deck
{
    private List<Card> cards = new List<Card>();
    
    public int RemainingCount => cards.Count;

    public Deck()
    {
        CreateDeck();
        Shuffle();
    }

    private void CreateDeck()
    {
        foreach(Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach(Rank rank in Enum.GetValues(typeof(Rank)))
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

        Card drawCard = cards[0];
        cards.RemoveAt(0);
        return drawCard;
    }

    public void ResetDeck()
    {
        CreateDeck();
        Shuffle();
    }
}
