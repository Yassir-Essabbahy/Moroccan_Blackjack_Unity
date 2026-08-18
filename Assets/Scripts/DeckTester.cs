using UnityEngine;

public class DeckTester : MonoBehaviour
{
    void Start()
    {
        Deck deck = new Deck();

        Debug.Log("Deck created. Cards remaining: " + deck.RemainingCount);

        Card firstCard = deck.DrawCard();
        Debug.Log("Drew: " + firstCard);
        Debug.Log("Cards remaining: " + deck.RemainingCount);

        Card secondCard = deck.DrawCard();
        Debug.Log("Drew: " + secondCard);
        Debug.Log("Cards remaining: " + deck.RemainingCount);

        Hand testHand = new Hand();
        testHand.AddCard(new Card(Suit.Suit1, Rank.One));   // Ace
        testHand.AddCard(new Card(Suit.Suit1, Rank.One));   // Ace
        testHand.AddCard(new Card(Suit.Suit1, Rank.Seven)); // 7

        Debug.Log("Score: " + testHand.GetScore()); // should be 19 (11+1+7)
        Debug.Log("Bust? " + testHand.IsBust());
    }
}