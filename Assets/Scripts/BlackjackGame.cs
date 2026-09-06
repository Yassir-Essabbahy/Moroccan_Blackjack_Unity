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
    [SerializeField] private FingerHealthController fingerHealth;
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
    [SerializeField] private TextMeshPro dealerScoreText;

    [Header("Loan Shark & Stakes")]
    [SerializeField] private int startingDebt = 5000;
    [SerializeField] private int debtPerWin = 1000;
    [SerializeField] private DebtHUD debtHUD;
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private MoneyStackManager moneyStackManager;

    [Header("Audio")]
    [SerializeField] private AudioSource cardAudioSource;
    [SerializeField] private AudioClip[] cardTakeSounds;
    [SerializeField] private Vector2 pitchVariation = new Vector2(0.92f, 1.08f);

    [Header("Dealing")]
    [SerializeField] private float timeBetweenCards = 0.4f;
    [SerializeField] private float revealDelay = 0.25f;
    [SerializeField] private float viewSwitchDelay = 0.4f;

    [Header("Rules")]
    [SerializeField] private bool dynamicAce = true;

    private Deck deck;
    private Hand playerHand;
    private Hand dealerHand;

    private int currentDebt;
    private int playerLives = 5;
    private int roundsPlayed;
    private int roundsWon;
    private int roundsLost;
    private bool isFinishingRound;
    private bool isDealingCard;

    [Header("Intro & Victory Sequences")]
    [SerializeField] private bool playIntroOnStart = true;
    [SerializeField] private IntroContractSequence introSequence;
    [SerializeField] private VictorySequenceController victorySequence;

    [Header("UI & Discard")]
    [SerializeField] private GameObject actionPanel;
    [SerializeField] private Transform discardPosition;

    public void Hit() => PlayerHit();
    public void Stand() => PlayerStand();

    public GameState CurrentState { get; private set; }

    private void Start()
    {
        if (cameraDirector == null)
            cameraDirector = GetComponent<BlackjackCameraDirector>();

        if (cardAudioSource == null)
            cardAudioSource = GetComponent<AudioSource>();
        if (cardAudioSource == null)
            cardAudioSource = gameObject.AddComponent<AudioSource>();

        if (debtHUD == null)
            debtHUD = FindAnyObjectByType<DebtHUD>();
        if (gameOverUI == null)
            gameOverUI = FindAnyObjectByType<GameOverUI>(FindObjectsInactive.Include);
        if (moneyStackManager == null)
            moneyStackManager = GetComponent<MoneyStackManager>();
        if (introSequence == null)
            introSequence = FindAnyObjectByType<IntroContractSequence>();
        if (victorySequence == null)
            victorySequence = FindAnyObjectByType<VictorySequenceController>();

        currentDebt = startingDebt;
        roundsPlayed = 0;
        roundsWon = 0;
        roundsLost = 0;

        deck = new Deck();
        playerHand = new Hand(dynamicAce);
        dealerHand = new Hand(dynamicAce);

        UpdateScoreDisplay();
        UpdateDebtDisplay();

        if (playIntroOnStart && introSequence != null && !introSequence.IsComplete)
        {
            introSequence.StartIntro();
        }
        else
        {
            StartRound();
        }
    }

    public bool IsContractSigned => introSequence == null || introSequence.IsComplete;

    // =========================================================
    // ROUND
    // =========================================================

    public void StartRound()
    {
        if (!IsContractSigned)
        {
            Log("[BlackjackGame] Cannot start round: Contract is not signed yet.");
            return;
        }

        if (CurrentState == GameState.GameOver || isFinishingRound)
        {
            Log("Match is over. No more rounds.");
            return;
        }

        roundsPlayed++;
        UpdateDebtDisplay();
        cameraDirector?.ShowTable();
        StartCoroutine(DealInitialCards());
    }

    private IEnumerator DealInitialCards()
    {
        // We are dealing cards, so player can't interact yet.
        isDealingCard = false;
        CurrentState = GameState.DealerTurn;
        SetActionPanelVisible(false);

        yield return AnimateClearTable();

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

        Log(
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
        UpdateScoreDisplay();


        Log(
            "Dealer starts with " +
            dealerHand.GetScore()
        );

        yield return WaitUntilCardDone(dealerCard);

        // -----------------------------------------------------
        // TABLE VIEW FOR THE REST OF THE ROUND
        // -----------------------------------------------------

        CurrentState = GameState.PlayerTurn;
        isDealingCard = false;
        SetActionPanelVisible(true);
        cameraDirector?.ShowTable();

        Log("Round started.");
        Log("Player: " + playerHand.GetScore());
        Log("Dealer: " + dealerHand.GetScore());
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

        Log(
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

        Log(
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
        if (!IsContractSigned || CurrentState != GameState.PlayerTurn || isDealingCard)
        {
            Log("Can't hit right now.");
            return;
        }

        StartCoroutine(PlayerHitRoutine());
    }

    private IEnumerator PlayerHitRoutine()
    {
        isDealingCard = true;
        SetActionPanelVisible(false);

        CardVisual playerCard = DealCardToPlayer(false);
        if (playerCard != null)
            yield return WaitUntilCardDone(playerCard);

        UpdateScoreDisplay();
        int score = playerHand.GetScore();

        Log("Player hit. Score: " + score);

        // Player busts immediately.
        if (playerHand.IsBust())
        {
            Log("Player busts!");
            isDealingCard = false;
            CurrentState = GameState.RoundResult;
            EvaluateRound();
            yield break;
        }

        // Auto-stand on 21 (prevents accidental extra hits causing ace downgrades or busts)
        if (score == 21)
        {
            Log("Player hit 21! Auto-standing.");
            isDealingCard = false;
            CurrentState = GameState.DealerTurn;
            StartCoroutine(DealerTurnRoutine());
            yield break;
        }

        // If player has filled all available slots, auto-stand
        if (playerSlots != null && playerHand.Cards.Count >= playerSlots.Count)
        {
            Log("Player reached max card slots. Auto-standing.");
            isDealingCard = false;
            CurrentState = GameState.DealerTurn;
            StartCoroutine(DealerTurnRoutine());
            yield break;
        }

        isDealingCard = false;
        SetActionPanelVisible(true);
    }

    public void PlayerStand()
    {
        if (!IsContractSigned || CurrentState != GameState.PlayerTurn || isDealingCard)
        {
            Log("Can't stand right now.");
            return;
        }

        CurrentState = GameState.DealerTurn;
        SetActionPanelVisible(false);

        StartCoroutine(DealerTurnRoutine());
    }

    // =========================================================
    // DEALER AI
    // =========================================================

    private IEnumerator DealerTurnRoutine()
    {
        Log(
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

            UpdateScoreDisplay();


            yield return WaitUntilCardDone(dealtCard);

            Log(
                "[DEALER] Draws. Score: " +
                dealerHand.GetScore()
            );

            // Stop immediately if dealer busts.
            if (dealerHand.IsBust())
            {
                Log("Dealer busts!");

                break;
            }
        }

        // Small pause before result.
        yield return new WaitForSeconds(0.5f);

        Log(
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
            Log("RESULT: Player busts. Dealer takes a finger.");
            playerLives--;
            roundsLost++;
        }
        else if (dealerHand.IsBust())
        {
            outcome = RoundOutcome.DealerBust;
            Log("RESULT: Dealer busts. Deducting " + debtPerWin + " DH from debt.");
            currentDebt = Mathf.Max(0, currentDebt - debtPerWin);
            roundsWon++;
            moneyStackManager?.DropMoneyStack(roundsWon - 1);
        }
        else if (playerScore > dealerScore)
        {
            outcome = RoundOutcome.PlayerWin;
            Log("RESULT: Player wins! " + playerScore + " vs " + dealerScore + ". Deducting " + debtPerWin + " DH.");
            currentDebt = Mathf.Max(0, currentDebt - debtPerWin);
            roundsWon++;
            moneyStackManager?.DropMoneyStack(roundsWon - 1);
        }
        else if (playerScore < dealerScore)
        {
            outcome = RoundOutcome.DealerWin;
            Log("RESULT: Dealer wins. " + playerScore + " vs " + dealerScore + ". Dealer takes a finger.");
            playerLives--;
            roundsLost++;
        }
        else
        {
            outcome = RoundOutcome.Push;
            Log("RESULT: Draw. " + playerScore + " vs " + dealerScore + ". Debt rolls over.");
        }

        UpdateDebtDisplay();
        CurrentState = GameState.RoundResult;
        isFinishingRound = true;
        StartCoroutine(FinishRoundRoutine(outcome));
    }

    private IEnumerator FinishRoundRoutine(RoundOutcome outcome)
    {
        UpdateDebtDisplay();

        int remainingFingers = GetCurrentFingers();
        bool isVictory = currentDebt <= 0;

        if (isVictory)
        {
            CurrentState = GameState.GameOver;
            if (dealerDialogue != null)
            {
                bool dialogueDone = false;
                yield return dealerDialogue.PlayGameOverSequence(cameraDirector, true, () => dialogueDone = true);
                yield return new WaitUntil(() => dialogueDone);
            }

            if (victorySequence != null)
            {
                yield return victorySequence.PlayVictorySequence();
            }

            Log("=== VICTORY: DEBT PAID IN FULL ===");

            isFinishingRound = false;
            yield break;
        }

        if (dealerDialogue != null)
        {
            bool completed = false;
            yield return dealerDialogue.Play(cameraDirector, outcome, () => completed = true);
            yield return new WaitUntil(() => completed);
        }

        if (penaltyStation != null && (outcome == RoundOutcome.DealerWin || outcome == RoundOutcome.PlayerBust))
        {
            yield return penaltyStation.PlayLossSequence();
        }

        UpdateDebtDisplay();

        remainingFingers = GetCurrentFingers();
        bool isDefeat = remainingFingers <= 0 || playerLives <= 0;

        if (isDefeat)
        {
            CurrentState = GameState.GameOver;
            if (dealerDialogue != null)
            {
                bool dialogueDone = false;
                yield return dealerDialogue.PlayGameOverSequence(cameraDirector, false, () => dialogueDone = true);
                yield return new WaitUntil(() => dialogueDone);
            }

            cameraDirector?.ShowTable();

            if (gameOverUI != null)
            {
                int debtCleared = startingDebt - currentDebt;
                gameOverUI.Show(this, false, roundsPlayed, remainingFingers, debtCleared);
            }
            else
            {
                Log("=== DEFEAT: OUT OF FINGERS ===");
            }

            isFinishingRound = false;
            yield break;
        }

        yield return new WaitForSeconds(nextRoundDelay);
        isFinishingRound = false;
        StartRound();
    }

    public void ResetMatch()
    {
        StopAllCoroutines();
        isFinishingRound = false;
        isDealingCard = false;
        CurrentState = GameState.RoundResult;

        currentDebt = startingDebt;
        playerLives = 5;
        roundsPlayed = 0;
        roundsWon = 0;
        roundsLost = 0;

        if (victorySequence != null)
            victorySequence.HideVictory();

        if (fingerHealth != null)
            fingerHealth.ResetForNewGame();

        if (gameOverUI != null)
            gameOverUI.HideImmediate();

        if (moneyStackManager != null)
            moneyStackManager.ClearMoneyStacks();

        deck = new Deck();
        playerHand = new Hand(dynamicAce);
        dealerHand = new Hand(dynamicAce);
        ClearTableInstant();

        UpdateScoreDisplay();
        UpdateDebtDisplay();

        StartRound();
    }

    public int GetCurrentFingers()
    {
        if (fingerHealth != null)
            return fingerHealth.CurrentFingers;
        return Mathf.Clamp(playerLives, 0, 5);
    }

    public void UpdateDebtDisplay()
    {
        if (debtHUD != null)
        {
            debtHUD.UpdateDisplay(currentDebt, GetCurrentFingers(), Mathf.Max(1, roundsPlayed));
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

        PlayCardTakeSound();

        visual.DealTo(
            slot,
            !faceDown,
            revealDelay
        );

        return visual;
    }

    private void PlayCardTakeSound()
    {
        if (cardTakeSounds == null || cardTakeSounds.Length == 0)
            return;

        AudioClip clip = cardTakeSounds[Random.Range(0, cardTakeSounds.Length)];
        if (clip == null)
            return;

        if (cardAudioSource != null)
        {
            cardAudioSource.pitch = Random.Range(pitchVariation.x, pitchVariation.y);
            cardAudioSource.PlayOneShot(clip);
        }
        else
        {
            Vector3 spawnPos = deckPosition != null ? deckPosition.position : transform.position;
            AudioSource.PlayClipAtPoint(clip, spawnPos);
        }
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
        if (dealerScoreText != null)
            dealerScoreText.text = dealerHand == null ? "0" : dealerHand.GetScore().ToString();

    }

    // =========================================================
    // UI & TABLE CLEANUP
    // =========================================================

    public void SetActionPanelVisible(bool visible)
    {
        if (actionPanel != null)
        {
            actionPanel.SetActive(visible);
        }
    }

    public IEnumerator AnimateClearTable()
    {
        SetActionPanelVisible(false);

        List<CardVisual> tableCards = new List<CardVisual>();
        GatherCardsFromSlots(playerSlots, tableCards);
        GatherCardsFromSlots(dealerSlots, tableCards);

        if (tableCards.Count > 0)
        {
            PlayCardTakeSound();
            Vector3 targetPos = discardPosition != null 
                ? discardPosition.position 
                : (deckPosition != null ? deckPosition.position + Vector3.up * 0.05f : transform.position);

            for (int i = 0; i < tableCards.Count; i++)
            {
                if (tableCards[i] != null)
                {
                    tableCards[i].DiscardAnimation(targetPos, 0.4f, i * 0.05f);
                }
            }

            yield return new WaitForSeconds(0.4f + (tableCards.Count * 0.05f));
        }

        ClearTableInstant();
    }

    private void GatherCardsFromSlots(List<Transform> slots, List<CardVisual> list)
    {
        if (slots == null) return;
        foreach (Transform slot in slots)
        {
            if (slot == null) continue;
            for (int i = 0; i < slot.childCount; i++)
            {
                var cv = slot.GetChild(i).GetComponent<CardVisual>();
                if (cv != null) list.Add(cv);
            }
        }
    }

    public void ClearTableInstant()
    {
        ClearSlots(playerSlots);
        ClearSlots(dealerSlots);
    }

    private void ClearSlots(List<Transform> slots)
    {
        if (slots == null) return;
        foreach (Transform slot in slots)
        {
            if (slot == null)
                continue;

            for (int i = slot.childCount - 1; i >= 0; i--)
            {
                Destroy(slot.GetChild(i).gameObject);
            }
        }
    }

    // =========================================================
    // DEBUG TEST SHORTCUTS & METHODS
    // =========================================================

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void DebugForceWin()
    {
        if (CurrentState == GameState.GameOver || isFinishingRound) return;
        StopAllCoroutines();
        SetActionPanelVisible(false);
        cameraDirector?.ShowTable();

        currentDebt = Mathf.Max(0, currentDebt - debtPerWin);
        roundsWon++;
        moneyStackManager?.DropMoneyStack(roundsWon - 1);
        UpdateDebtDisplay();
        CurrentState = GameState.RoundResult;
        isFinishingRound = true;
        StartCoroutine(FinishRoundRoutine(RoundOutcome.PlayerWin));
    }

    public void DebugForceLoss()
    {
        if (CurrentState == GameState.GameOver || isFinishingRound) return;
        StopAllCoroutines();
        SetActionPanelVisible(false);
        cameraDirector?.ShowTable();

        playerLives--;
        roundsLost++;
        UpdateDebtDisplay();
        CurrentState = GameState.RoundResult;
        isFinishingRound = true;
        StartCoroutine(FinishRoundRoutine(RoundOutcome.DealerWin));
    }

    public void DebugInstantVictory()
    {
        currentDebt = 0;
        DebugForceWin();
    }

    public void DebugInstantDefeat()
    {
        playerLives = 1;
        DebugForceLoss();
    }
#endif

    [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private static void Log(string message)
    {
        Debug.Log(message);
    }
}
