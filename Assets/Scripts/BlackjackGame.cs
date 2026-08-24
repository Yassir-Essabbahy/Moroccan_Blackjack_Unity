using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public enum RoundOutcome
{
    PlayerWin,
    DealerWin,
    Push,
    PlayerBust,
    DealerBust
}

public class BlackjackGame : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private BlackjackCameraDirector cameraDirector;

    [Header("Dealer Dialogue")]
    [SerializeField] private DealerDialogueSequence dealerDialogue;
    [SerializeField] private PenaltyStation penaltyStation;
    [SerializeField] private float nextRoundDelay = 0.25f;

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
    [SerializeField] private float viewSwitchDelay = 0.4f;

    private Deck deck;
    private Hand playerHand;
    private Hand dealerHand;

    private int playerLives = 5;
    private int dealerLives = 5;
    private bool isFinishingRound;

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
        if (CurrentState == GameState.GameOver || isFinishingRound)
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
        // PLAYER: ONE CARD — wait until it lands and flips
        // -----------------------------------------------------

        cameraDirector?.ShowPlayer();

        yield return new WaitForSeconds(viewSwitchDelay);

        CardVisual playerCard =
            DealCardToPlayer(false);

        UpdateScoreDisplay();

        Debug.Log(
            "Player starts with " +
            playerHand.GetScore()
        );

        yield return WaitUntilCardDone(playerCard);

        // -----------------------------------------------------
        // DEALER: ONE CARD — wait until it lands and flips
        // -----------------------------------------------------

        cameraDirector?.ShowDealer();

        yield return new WaitForSeconds(viewSwitchDelay);

        CardVisual dealerCard =
            DealCardToDealer(false);

        Debug.Log(
            "Dealer starts with " +
            dealerHand.GetScore()
        );

        yield return WaitUntilCardDone(dealerCard);

        // -----------------------------------------------------
        // TABLE VIEW FOR THE REST OF THE ROUND
        // -----------------------------------------------------

        CurrentState = GameState.PlayerTurn;
        cameraDirector?.ShowTable();

        Debug.Log("Round started.");
        Debug.Log("Player: " + playerHand.GetScore());
        Debug.Log("Dealer: " + dealerHand.GetScore());
    }

    private IEnumerator WaitUntilCardDone(
        CardVisual card
    )
    {
        if (card == null)
            yield break;

        yield return new WaitUntil(
            () => card.DealFinished
        );
    }

    // =========================================================
    // DEAL CARDS
    // =========================================================

    private CardVisual DealCardToPlayer(bool faceDown)
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

            return null;
        }

        Debug.Log(
            "[DEAL] Player drew: " +
            card +
            " (Value=" +
            card.Value +
            ") -> slot " +
            slotIndex
        );

        return SpawnCardVisual(
            card,
            playerSlots[slotIndex],
            faceDown
        );
    }

    private CardVisual DealCardToDealer(bool faceDown)
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

            return null;
        }

        Debug.Log(
            "[DEAL] Dealer drew: " +
            card +
            " (Value=" +
            card.Value +
            ") -> slot " +
            slotIndex
        );

        return SpawnCardVisual(
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

        StartCoroutine(DealerTurnRoutine());
    }

    // =========================================================
    // DEALER AI
    // =========================================================

    private IEnumerator DealerTurnRoutine()
    {
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

            CardVisual dealtCard =
                DealCardToDealer(false);

            yield return WaitUntilCardDone(dealtCard);

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
        RoundOutcome outcome;

        if (playerHand.IsBust())
        {
            outcome = RoundOutcome.PlayerBust;
            Debug.Log("RESULT: Player busts. Dealer wins the round.");
            playerLives--;
        }
        else if (dealerHand.IsBust())
        {
            outcome = RoundOutcome.DealerBust;
            Debug.Log("RESULT: Dealer busts. Player wins the round.");
            dealerLives--;
        }
        else if (playerScore > dealerScore)
        {
            outcome = RoundOutcome.PlayerWin;
            Debug.Log("RESULT: Player wins! " + playerScore + " vs " + dealerScore);
            dealerLives--;
        }
        else if (playerScore < dealerScore)
        {
            outcome = RoundOutcome.DealerWin;
            Debug.Log("RESULT: Dealer wins. " + playerScore + " vs " + dealerScore);
            playerLives--;
        }
        else
        {
            outcome = RoundOutcome.Push;
            Debug.Log("RESULT: Draw. " + playerScore + " vs " + dealerScore + ". No lives lost.");
        }

        Debug.Log("Lives — Player: " + playerLives + " | Dealer: " + dealerLives);
        bool gameOver = playerLives <= 0 || dealerLives <= 0;
        CurrentState = gameOver ? GameState.GameOver : GameState.RoundResult;
        isFinishingRound = true;
        StartCoroutine(FinishRoundRoutine(outcome, gameOver));
    }

    private IEnumerator FinishRoundRoutine(RoundOutcome outcome, bool gameOver)
    {
        if (penaltyStation != null && (outcome == RoundOutcome.DealerWin || outcome == RoundOutcome.PlayerBust))
            yield return penaltyStation.PlayLossSequence();

        {
            bool completed = false;
            yield return dealerDialogue.Play(cameraDirector, outcome, () => completed = true);
            yield return new WaitUntil(() => completed);
        }

        if (gameOver)
        {
            isFinishingRound = false;
            yield break;
        }

        yield return new WaitForSeconds(nextRoundDelay);
        isFinishingRound = false;
        StartRound();
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
