using System.Collections.Generic;

public class Hand
{
    private readonly List<Card> cards = new List<Card>();

    public IReadOnlyList<Card> Cards => cards;
    public bool DynamicAce { get; set; } = true;

    public Hand(bool dynamicAce = true)
    {
        DynamicAce = dynamicAce;
    }

    public void AddCard(Card card)
    {
        cards.Add(card);
    }

    public void ClearHand()
    {
        cards.Clear();
    }

    public int GetScore()
    {
        int total = 0;
        int aceCount = 0;

        foreach (Card card in cards)
        {
            if (card.Rank == Rank.One)
            {
                total += DynamicAce ? 11 : 1;
                aceCount++;
            }
            else
            {
                total += card.Value;
            }
        }

        if (DynamicAce)
        {
            while (total > 21 && aceCount > 0)
            {
                total -= 10;
                aceCount--;
            }
        }

        return total;
    }

    public bool IsBust()
    {
        return GetScore() > 21;
    }

    public bool IsBlackjack()
    {
        return cards.Count == 2 && GetScore() == 21;
    }

    public bool HasTwentyOne()
    {
        return GetScore() == 21;
    }

    public bool IsSoft17()
    {
        if (GetScore() != 17 || !DynamicAce)
            return false;

        int hardTotal = 0;
        int aceCount = 0;
        foreach (Card card in cards)
        {
            if (card.Rank == Rank.One)
            {
                hardTotal += 1;
                aceCount++;
            }
            else
            {
                hardTotal += card.Value;
            }
        }

        return aceCount > 0 && hardTotal == 7;
    }
}
