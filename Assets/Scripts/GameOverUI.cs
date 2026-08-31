using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("Canvas Group")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.8f;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button quitButton;

    [Header("Colors")]
    [SerializeField] private Color victoryColor = new Color(0.35f, 0.95f, 0.45f, 1f);
    [SerializeField] private Color defeatColor = new Color(0.95f, 0.25f, 0.2f, 1f);

    private BlackjackGame gameInstance;
    private bool isGameOverActive;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        HideImmediate();

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void Update()
    {
        if (!isGameOverActive)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            OnPlayAgainClicked();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnQuitClicked();
        }
    }

    public void Show(BlackjackGame game, bool isVictory, int roundsPlayed, int fingersLeft, int debtCleared)
    {
        gameInstance = game;
        isGameOverActive = true;

        if (titleText != null)
        {
            titleText.text = isVictory ? "DEBT SETTLED" : "DEFAULTED";
            titleText.color = isVictory ? victoryColor : defeatColor;
        }

        if (subtitleText != null)
        {
            subtitleText.text = isVictory
                ? "You paid your debt in full and walk away alive."
                : "Five broken fingers and an empty wallet. You have nothing left.";
        }

        if (statsText != null)
        {
            statsText.text = $"<b>Rounds Played:</b> {roundsPlayed}\n" +
                             $"<b>Fingers Saved:</b> {fingersLeft}/5\n" +
                             $"<b>Debt Cleared:</b> {debtCleared:N0} DH";
        }

        StopAllCoroutines();
        StartCoroutine(FadeRoutine(1f));
    }

    public void HideImmediate()
    {
        isGameOverActive = false;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnPlayAgainClicked()
    {
        HideImmediate();
        if (gameInstance != null)
        {
            gameInstance.ResetMatch();
        }
    }

    public void OnQuitClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (canvasGroup == null)
            yield break;

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = targetAlpha > 0.5f;
        canvasGroup.blocksRaycasts = targetAlpha > 0.5f;
    }
}