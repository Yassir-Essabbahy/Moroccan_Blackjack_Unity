using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class BlackjackGame : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private BlackjackCameraDirector cameraDirector;

    [Header("Card")]
    [SerializeField] private GameObject cardPrefab;

    [SerializeField] private Transform deckPosition;

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

    public GameState CurrentState { get; private set; }

    private void Start()
    {
        if (cameraDirector == null)
            cameraDirector = GetComponent<BlackjackCameraDirector>();

        deck = new Deck();
        playerHand = new Hand();
        dealerHand = new Hand();

        UpdateScoreDisplay();

        StartRound();
    }

    // =========================================================
    // ROUND
    // =========================================================

    public void StartRound()
    {
        if (CurrentState == GameState.GameOver)
        {
            Debug.Log("Match is over. No more rounds.");
            return;
        }

        cameraDirector?.ShowTable();
        StartCoroutine(DealInitialCards());
    }

    private IEnumerator DealInitialCards()
    {
        // We are dealing cards, so player can't interact yet.
        CurrentState = GameState.DealerTurn;

        ClearTable();

        playerHand.ClearHand();
        dealerHand.ClearHand();

        UpdateScoreDisplay();

        // -----------------------------------------------------
        // PLAYER: ONE CARD
        // -----------------------------------------------------

        cameraDirector?.ShowPlayer();
        DealCardToPlayer(false);

        UpdateScoreDisplay();

        Debug.Log(
            "Player starts with " +
            playerHand.GetScore()
        );

        yield return new WaitForSeconds(timeBetweenCards);

        // -----------------------------------------------------
        // DEALER: ONE CARD
        // -----------------------------------------------------

        cameraDirector?.ShowDealer();
        DealCardToDealer(false);

        Debug.Log(
            "Dealer starts with " +
            dealerHand.GetScore()
        );

        yield return new WaitForSeconds(revealDelay);

        // -----------------------------------------------------
        // PLAYER TURN
        // -----------------------------------------------------

        CurrentState = GameState.PlayerTurn;
        cameraDirector?.ShowPlayer();

        Debug.Log("Round started.");
        Debug.Log("Player: " + playerHand.GetScore());
        Debug.Log("Dealer: " + dealerHand.GetScore());
    }

    // =========================================================
    // DEAL CARDS
    // =========================================================

    private void DealCardToPlayer(bool faceDown)
    {
        Card card = deck.DrawCard();

        playerHand.AddCard(card);

        int slotIndex = playerHand.Cards.Count - 1;

        if (slotIndex >= playerSlots.Count)
        {
            Debug.LogError(
                "Not enough player slots! " +
                "Player needs slot " +
                slotIndex
            );

            return;
        }

        Debug.Log(
            "[DEAL] Player drew: " +
            card +
            " (Value=" +
            card.Value +
            ") -> slot " +
            slotIndex
        );

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
            Debug.LogError(
                "Not enough dealer slots! " +
                "Dealer needs slot " +
                slotIndex
            );

            return;
        }

        Debug.Log(
            "[DEAL] Dealer drew: " +
            card +
            " (Value=" +
            card.Value +
            ") -> slot " +
            slotIndex
        );

        SpawnCardVisual(
            card,
            dealerSlots[slotIndex],
            faceDown
        );
    }

    // =========================================================
    // PLAYER ACTIONS
    // =========================================================

    public void PlayerHit()
    {
        if (CurrentState != GameState.PlayerTurn)
        {
            Debug.Log("Can't hit right now.");
            return;
        }

        cameraDirector?.ShowPlayer();
        DealCardToPlayer(false);

        UpdateScoreDisplay();

        Debug.Log(
            "Player hit. Score: " +
            playerHand.GetScore()
        );

        // Player busts immediately.
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
        cameraDirector?.ShowDealer();

        StartCoroutine(DealerTurnRoutine());
    }

    // =========================================================
    // DEALER AI
    // =========================================================

    private IEnumerator DealerTurnRoutine()
    {
        cameraDirector?.ShowDealer();
        Debug.Log(
            "Dealer begins turn with " +
            dealerHand.GetScore()
        );

        // If the player already busted, there is no reason
        // for the dealer to draw.
        if (playerHand.IsBust())
        {
            CurrentState = GameState.RoundResult;

            EvaluateRound();

            yield break;
        }

        // Dealer keeps drawing until reaching 17.
        // Soft 17 is treated as a hit.
        while (
            dealerHand.GetScore() < 17 ||
            dealerHand.IsSoft17()
        )
        {
            yield return new WaitForSeconds(
                timeBetweenCards
            );

            DealCardToDealer(false);

            Debug.Log(
                "[DEALER] Draws. Score: " +
                dealerHand.GetScore()
            );

            // Stop immediately if dealer busts.
            if (dealerHand.IsBust())
            {
                Debug.Log("Dealer busts!");

                break;
            }
        }

        // Small pause before result.
        yield return new WaitForSeconds(0.5f);

        Debug.Log(
            "Dealer stands with " +
            dealerHand.GetScore()
        );

        CurrentState = GameState.RoundResult;

        EvaluateRound();
    }

    // =========================================================
    // ROUND RESULT
    // =========================================================

    private void EvaluateRound()
    {
        cameraDirector?.ShowTable();
        int playerScore = playerHand.GetScore();
        int dealerScore = dealerHand.GetScore();

        if (playerHand.IsBust())
        {
            Debug.Log(
                "RESULT: Player busts. " +
                "Dealer wins the round."
            );

            playerLives--;
        }
        else if (dealerHand.IsBust())
        {
            Debug.Log(
                "RESULT: Dealer busts. " +
                "Player wins the round."
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

        // -----------------------------------------------------
        // GAME OVER
        // -----------------------------------------------------

        if (playerLives <= 0 || dealerLives <= 0)
        {
            CurrentState = GameState.GameOver;

            if (
                playerLives <= 0 &&
                dealerLives <= 0
            )
            {
                Debug.Log(
                    "GAME OVER — " +
                    "Both ran out at once. Draw match."
                );
            }
            else if (playerLives <= 0)
            {
                Debug.Log(
                    "GAME OVER — " +
                    "Dealer wins the match!"
                );
            }
            else
            {
                Debug.Log(
                    "GAME OVER — " +
                    "Player wins the match!"
                );
            }
        }
        else
        {
            CurrentState = GameState.RoundResult;
        }
    }

    // =========================================================
    // CARD VISUAL
    // =========================================================

    private CardVisual SpawnCardVisual(
        Card card,
        Transform slot,
        bool faceDown = false
    )
    {
        if (deckPosition == null)
        {
            Debug.LogError(
                "Deck Position is not assigned on BlackjackGame!"
            );

            return null;
        }

        GameObject cardObject = Instantiate(
            cardPrefab,
            deckPosition.position,
            deckPosition.rotation
        );

        CardVisual visual =
            cardObject.GetComponent<CardVisual>();

        if (visual == null)
        {
            Debug.LogError(
                "Card prefab is missing " +
                "CardVisual component!"
            );

            return null;
        }

        // The visual system handles the artwork.
        // Every dealt card leaves the deck face-down.
        // Normal cards flip after reaching their slot.
        visual.SetCard(card, true);

        visual.DealTo(
            slot,
            !faceDown,
            revealDelay
        );

        return visual;
    }

    // =========================================================
    // SCORE
    // =========================================================

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

    // =========================================================
    // TABLE CLEANUP
    // =========================================================

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
