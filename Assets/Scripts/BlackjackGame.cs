using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class BlackjackGame : MonoBehaviour
{
    [Header("Card")]
    [SerializeField] private GameObject cardPrefab;

    [Header("Player Card Slots")]
    [SerializeField] private List<Transform> playerSlots =
        new List<Transform>();

    [Header("Dealer Card Slots")]
    [SerializeField] private List<Transform> dealerSlots =
        new List<Transform>();

    [Header("Score Display")]
    [SerializeField] private TextMeshPro scoreText;

    [Header("Dealing")]
    [SerializeField] private float timeBetweenCards = 0.4f;
    [SerializeField] private float revealDelay = 0.25f;

    private Deck deck;
    private Hand playerHand;
    private Hand dealerHand;

    private int playerLives = 5;
    private int dealerLives = 5;

    private CardVisual hiddenDealerCardVisual;

    public GameState CurrentState { get; private set; }

    private void Start()
    {
        deck = new Deck();
        playerHand = new Hand();
        dealerHand = new Hand();

        UpdateScoreDisplay();

        StartRound();
    }

    public void StartRound()
    {
        if (CurrentState == GameState.GameOver)
        {
            Debug.Log("Match is over. No more rounds.");
            return;
        }

        StartCoroutine(DealInitialCards());
    }

    private IEnumerator DealInitialCards()
    {
        CurrentState = GameState.DealerTurn;

        ClearTable();

        playerHand.ClearHand();
        dealerHand.ClearHand();

        hiddenDealerCardVisual = null;

        UpdateScoreDisplay();

        // Player card 1
        DealCardToPlayer(false);

        UpdateScoreDisplay();

        yield return new WaitForSeconds(timeBetweenCards);

        // Dealer card 1
        DealCardToDealer(false);

        yield return new WaitForSeconds(timeBetweenCards);

        // Player card 2
        DealCardToPlayer(false);

        UpdateScoreDisplay();

        yield return new WaitForSeconds(timeBetweenCards);

        // Dealer card 2 - hidden
        DealCardToDealer(true);

        yield return new WaitForSeconds(revealDelay);

        CurrentState = GameState.PlayerTurn;

        Debug.Log("Round started.");
        Debug.Log("Player: " + playerHand.GetScore());
        Debug.Log("Dealer shows: " + dealerHand.Cards[0]);
    }

    private void DealCardToPlayer(bool faceDown)
    {
        Card card = deck.DrawCard();

        playerHand.AddCard(card);

        int slotIndex = playerHand.Cards.Count - 1;

        if (slotIndex >= playerSlots.Count)
        {
            Debug.LogError("Not enough player slots!");
            return;
        }

        SpawnCardVisual(
            card,
            playerSlots[slotIndex],
            faceDown
        );
    }

    private void DealCardToDealer(bool faceDown)
    {
        Card card = deck.DrawCard();

        dealerHand.AddCard(card);

        int slotIndex = dealerHand.Cards.Count - 1;

        if (slotIndex >= dealerSlots.Count)
        {
            Debug.LogError("Not enough dealer slots!");
            return;
        }

        CardVisual visual = SpawnCardVisual(
            card,
            dealerSlots[slotIndex],
            faceDown
        );

        if (faceDown)
        {
            hiddenDealerCardVisual = visual;
        }
    }

    public void PlayerHit()
    {
        if (CurrentState != GameState.PlayerTurn)
        {
            Debug.Log("Can't hit right now.");
            return;
        }

        DealCardToPlayer(false);

        UpdateScoreDisplay();

        Debug.Log(
            "Player hit. Score: " +
            playerHand.GetScore()
        );

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
        Debug.Log(
            "Dealer reveals: " +
            dealerHand.Cards[1]
        );

        if (hiddenDealerCardVisual != null)
        {
            hiddenDealerCardVisual.Reveal();
        }

        if (playerHand.IsBust())
        {
            CurrentState = GameState.RoundResult;

            EvaluateRound();

            return;
        }

        while (
            dealerHand.GetScore() < 17 ||
            dealerHand.IsSoft17()
        )
        {
            DealCardToDealer(false);

            Debug.Log(
                "Dealer draws. Score: " +
                dealerHand.GetScore()
            );

            if (dealerHand.IsBust())
            {
                break;
            }
        }

        Debug.Log(
            "Dealer stands with " +
            dealerHand.GetScore()
        );

        CurrentState = GameState.RoundResult;

        EvaluateRound();
    }

    private void EvaluateRound()
    {
        int playerScore = playerHand.GetScore();
        int dealerScore = dealerHand.GetScore();

        if (playerHand.IsBust())
        {
            Debug.Log(
                "RESULT: Player busts. Dealer wins the round."
            );

            playerLives--;
        }
        else if (dealerHand.IsBust())
        {
            Debug.Log(
                "RESULT: Dealer busts. Player wins the round."
            );

            dealerLives--;
        }
        else if (playerScore > dealerScore)
        {
            Debug.Log(
                "RESULT: Player wins! " +
                playerScore +
                " vs " +
                dealerScore
            );

            dealerLives--;
        }
        else if (playerScore < dealerScore)
        {
            Debug.Log(
                "RESULT: Dealer wins. " +
                playerScore +
                " vs " +
                dealerScore
            );

            playerLives--;
        }
        else
        {
            Debug.Log(
                "RESULT: Draw. " +
                playerScore +
                " vs " +
                dealerScore +
                ". No lives lost."
            );
        }

        Debug.Log(
            "Lives — Player: " +
            playerLives +
            " | Dealer: " +
            dealerLives
        );

        if (playerLives <= 0 || dealerLives <= 0)
        {
            CurrentState = GameState.GameOver;

            if (playerLives <= 0 && dealerLives <= 0)
            {
                Debug.Log(
                    "GAME OVER — Both ran out at once. Draw match."
                );
            }
            else if (playerLives <= 0)
            {
                Debug.Log(
                    "GAME OVER — Dealer wins the match!"
                );
            }
            else
            {
                Debug.Log(
                    "GAME OVER — Player wins the match!"
                );
            }
        }
        else
        {
            CurrentState = GameState.RoundResult;
        }
    }

    private CardVisual SpawnCardVisual(
        Card card,
        Transform slot,
        bool faceDown = false
    )
    {
        GameObject cardObject = Instantiate(
            cardPrefab,
            slot.position,
            slot.rotation,
            slot
        );

        CardVisual visual =
            cardObject.GetComponent<CardVisual>();

        if (visual == null)
        {
            Debug.LogError(
                "Card prefab is missing CardVisual component!"
            );

            return null;
        }

        // Every card starts face-down.
        visual.SetCard(card, true);

        // Normal cards reveal automatically.
        if (!faceDown)
        {
            StartCoroutine(
                RevealCardAfterDelay(visual)
            );
        }

        return visual;
    }

    private IEnumerator RevealCardAfterDelay(
        CardVisual visual
    )
    {
        yield return new WaitForSeconds(revealDelay);

        if (visual != null)
        {
            visual.Reveal();
        }
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText == null)
            return;

        if (playerHand == null)
        {
            scoreText.text = "0";
            return;
        }

        scoreText.text =
            playerHand.GetScore().ToString();
    }

    private void ClearTable()
    {
        ClearSlots(playerSlots);
        ClearSlots(dealerSlots);
    }

    private void ClearSlots(
        List<Transform> slots
    )
    {
        foreach (Transform slot in slots)
        {
            if (slot == null)
                continue;

            for (
                int i = slot.childCount - 1;
                i >= 0;
                i--
            )
            {
                Destroy(
                    slot.GetChild(i).gameObject
                );
            }
        }
    }
}