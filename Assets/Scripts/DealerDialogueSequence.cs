using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DealerDialogueSequence : MonoBehaviour
{
    [Serializable]
    private class DialogueOption
    {
        [TextArea(2, 4)]
        public string line;
        public AudioClip voice;
    }

    [Header("Presentation")]
    [SerializeField] private CanvasGroup dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Animator dealerAnimator;
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private string talkingAnimation = "Talking";
    [SerializeField] private string reactingAnimation = "Reacting";
    [SerializeField] private string idleAnimation = "Idle";
    [SerializeField] private float cameraTransitionDuration = 0.75f;
    [SerializeField] private float minimumReadableDuration = 2f;
    [SerializeField] private float charactersPerSecond = 18f;

    [Header("Result Dialogue")]
    [SerializeField] private DialogueOption[] playerWins =
    {
        new DialogueOption { line = "Well played. Let’s see if luck stays with you." }
    };
    [SerializeField] private DialogueOption[] dealerWins =
    {
        new DialogueOption { line = "The house takes this one." }
    };
    [SerializeField] private DialogueOption[] pushes =
    {
        new DialogueOption { line = "A tie. We go again." }
    };
    [SerializeField] private DialogueOption[] playerBusts =
    {
        new DialogueOption { line = "Too risky. Better luck next round." }
    };
    [SerializeField] private DialogueOption[] dealerBusts =
    {
        new DialogueOption { line = "You got me this time." }
    };

    public IEnumerator Play(BlackjackCameraDirector cameraDirector, RoundOutcome outcome, Action onComplete)
    {
        DialogueOption option = Pick(GetOptions(outcome));
        string line = option == null ? string.Empty : option.line;

        cameraDirector?.ShowDealer();
        yield return new WaitForSeconds(cameraTransitionDuration);

        SetAnimation(outcome);
        Show(line);
        PlayVoice(option == null ? null : option.voice);

        float readableDuration = Mathf.Max(minimumReadableDuration, line.Length / Mathf.Max(1f, charactersPerSecond));
        float voiceDuration = voiceSource != null && voiceSource.clip != null ? voiceSource.clip.length : 0f;
        yield return new WaitForSeconds(Mathf.Max(readableDuration, voiceDuration));

        if (voiceSource != null)
            voiceSource.Stop();
        Hide();
        SetAnimationName(idleAnimation);
        cameraDirector?.ShowTable();
        yield return new WaitForSeconds(cameraTransitionDuration);
        onComplete?.Invoke();
    }

    private DialogueOption[] GetOptions(RoundOutcome outcome)
    {
        switch (outcome)
        {
            case RoundOutcome.PlayerBust: return playerBusts;
            case RoundOutcome.DealerBust: return dealerBusts;
            case RoundOutcome.PlayerWin: return playerWins;
            case RoundOutcome.DealerWin: return dealerWins;
            default: return pushes;
        }
    }

    private static DialogueOption Pick(DialogueOption[] options)
    {
        if (options == null || options.Length == 0)
            return null;
        return options[UnityEngine.Random.Range(0, options.Length)];
    }

    private void Show(string line)
    {
        if (dialogueText != null)
            dialogueText.text = line;
        if (dialoguePanel != null)
        {
            dialoguePanel.alpha = 1f;
            dialoguePanel.interactable = false;
            dialoguePanel.blocksRaycasts = true;
        }
    }

    private void Hide()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.alpha = 0f;
            dialoguePanel.blocksRaycasts = false;
        }
        if (dialogueText != null)
            dialogueText.text = string.Empty;
    }

    private void PlayVoice(AudioClip clip)
    {
        if (voiceSource == null || clip == null)
            return;
        voiceSource.clip = clip;
        voiceSource.Play();
    }

    private void SetAnimation(RoundOutcome outcome)
    {
        string animation = outcome == RoundOutcome.PlayerWin || outcome == RoundOutcome.DealerBust
            ? reactingAnimation
            : talkingAnimation;
        SetAnimationName(string.IsNullOrWhiteSpace(animation) ? idleAnimation : animation);
    }

    private void SetAnimationName(string animation)
    {
        if (dealerAnimator != null && !string.IsNullOrWhiteSpace(animation))
            dealerAnimator.Play(animation);
    }
}
