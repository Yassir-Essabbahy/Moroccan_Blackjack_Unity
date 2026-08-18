using UnityEngine;

public enum GameState
{
    Waiting,
    PlayerTurn,
    DealerTurn,
    RoundResult,
    GameOver
}

public class BlackjackGame : MonoBehaviour
{
   private Deck deck;
   private Hand playerHand;
   private Hand dealerHand;

   private int playerLives = 5;
   private int dealerLives = 5;

   public GameState CurrentState { get; private set; }

   void Start()
   {
    deck = new Deck();
    playerHand = new Hand();
    dealerHand = new Hand();

    StartRound();

   }

   public void StartRound()
   {

    if (CurrentState == GameState.GameOver)
    {
        Debug.Log("Match is over. No more rounds.");
        return;
    }
        playerHand.ClearHand();
        dealerHand.ClearHand();

        playerHand.AddCard(deck.DrawCard());
        playerHand.AddCard(deck.DrawCard());

        dealerHand.AddCard(deck.DrawCard());
        dealerHand.AddCard(deck.DrawCard());

        CurrentState = GameState.PlayerTurn;

        Debug.Log("Round Started.");
        Debug.Log("Player: " + playerHand.GetScore());
        Debug.Log("Dealer shows: " + dealerHand.Cards[0]);

   }

   public void PlayerHit()
   {
        if (CurrentState != GameState.PlayerTurn)
        {
            Debug.Log("Can't hit right now.");
            return;
        }

        playerHand.AddCard(deck.DrawCard());
        Debug.Log("Player hit. Score: " + playerHand.GetScore());

        if (playerHand.IsBust())
        {
            Debug.Log("Player busts!");
            CurrentState = GameState.RoundResult;
            EvaluateRound();
        }
   }

   public void PlayerStand()
   {
    if (CurrentState != GameState.PlayerTurn)
    {
        Debug.Log("Can't stand right now.");
        return;
    }

    CurrentState = GameState.DealerTurn;
    DealerTurn();

   }

   private void DealerTurn()
   {
    Debug.Log("Dealer reveals: " + dealerHand.Cards[1]);

    while (dealerHand.GetScore() < 17)
    {
        dealerHand.AddCard(deck.DrawCard());
        Debug.Log("Dealer draws. Score: " + dealerHand.GetScore());
    }

    CurrentState = GameState.RoundResult;
    EvaluateRound();

   }

    private void EvaluateRound()
    {
        int playerScore = playerHand.GetScore();
        int dealerScore = dealerHand.GetScore();

        if (playerHand.IsBust())
        {
            Debug.Log("RESULT: Player busts. Dealer wins the round.");
            playerLives--;
        }
        else if (dealerHand.IsBust())
        {
            Debug.Log("RESULT: Dealer busts. Player wins the round.");
            dealerLives--;
        }
        else if (playerScore > dealerScore)
        {
            Debug.Log("RESULT: Player wins! " + playerScore + " vs " + dealerScore);
            dealerLives--;
        }
        else if (playerScore < dealerScore)
        {
            Debug.Log("RESULT: Dealer wins. " + playerScore + " vs " + dealerScore);
            playerLives--;
        }
        else
        {
            Debug.Log("RESULT: Draw. " + playerScore + " vs " + dealerScore + ". No lives lost.");
        }

        Debug.Log("Lives — Player: " + playerLives + " | Dealer: " + dealerLives);

        if (playerLives <= 0 || dealerLives <= 0)
        {
            CurrentState = GameState.GameOver;

            if (playerLives <= 0 && dealerLives <= 0)
                Debug.Log("GAME OVER — Both ran out at once. Draw match.");
            else if (playerLives <= 0)
                Debug.Log("GAME OVER — Dealer wins the match!");
            else
                Debug.Log("GAME OVER — Player wins the match!");
        }
        else
        {
            CurrentState = GameState.RoundResult;
        }
    }
}
