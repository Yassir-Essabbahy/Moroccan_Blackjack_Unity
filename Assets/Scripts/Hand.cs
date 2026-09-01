using System.Collections.Generic;

public class Hand
{
    private readonly List<Card> cards = new List<Card>();

    public IReadOnlyList<Card> Cards => cards;

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
            total += card.Value;

            if (card.Rank == Rank.One)
            {
                aceCount++;
            }
        }

        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }

        return total;
    }

    public bool IsBust()
    {
        return GetScore() > 21;
    }

    public bool IsBlackjack()
    {
        return GetScore() == 21;
    }

    public bool IsSoft17()
    {
        if (GetScore() != 17)
            return false;

        foreach (Card card in cards)
        {
            if (card.Rank == Rank.One)
                return true;
        }

        return false;
    }
}
