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
    }
}