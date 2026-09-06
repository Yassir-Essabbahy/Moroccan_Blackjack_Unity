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
    [SerializeField] private ReactiveMusicController reactiveMusic;
    [SerializeField] private string talkingAnimation = "Talking";
    [SerializeField] private string reactingAnimation = "Reacting";
    [SerializeField] private string idleAnimation = "Idle";
    [SerializeField] private float cameraTransitionDuration = 0.75f;
    [SerializeField] private float minimumReadableDuration = 2f;
    [SerializeField] private float charactersPerSecond = 18f;

    [Header("Result Dialogue")]
    [SerializeField] private DialogueOption[] playerWins =
    {
        new DialogueOption { line = "That takes 1,000 DH off your tab. But you're not out yet." },
        new DialogueOption { line = "One step closer to the door. Keep going." },
        new DialogueOption { line = "A good hand. Your debt shrinks... for now." }
    };
    [SerializeField] private DialogueOption[] dealerWins =
    {
        new DialogueOption { line = "Interest is due. Place your hand on the table." },
        new DialogueOption { line = "You can't pay in coin? You pay in bone." },
        new DialogueOption { line = "The loan doesn't forgive, and neither do I." }
    };
    [SerializeField] private DialogueOption[] pushes =
    {
        new DialogueOption { line = "A tie. Your debt rolls over to the next hand." },
        new DialogueOption { line = "Nobody wins. Put the cards back." }
    };
    [SerializeField] private DialogueOption[] playerBusts =
    {
        new DialogueOption { line = "Greedy. You overplayed your hand... hand over a finger." },
        new DialogueOption { line = "Bust. That is going to cost you dearly." }
    };
    [SerializeField] private DialogueOption[] dealerBusts =
    {
        new DialogueOption { line = "Tch. I overreached. Deduct 1,000 DH from your ledger." }
    };

    [Header("End Game Dialogue")]
    [SerializeField] private DialogueOption[] victoryLines =
    {
        new DialogueOption { line = "Your debt is settled in full. Take your hands and get out." }
    };
    [SerializeField] private DialogueOption[] defeatLines =
    {
        new DialogueOption { line = "Five broken fingers and an empty wallet. You have nothing left." }
    };

    private void Awake()
    {
        if (reactiveMusic == null)
            reactiveMusic = GetComponent<ReactiveMusicController>();
    }
    public IEnumerator Play(BlackjackCameraDirector cameraDirector, RoundOutcome outcome, Action onComplete)
    {
        DialogueOption option = Pick(GetOptions(outcome));
        string line = option == null ? string.Empty : option.line;

        yield return reactiveMusic != null
            ? reactiveMusic.TransitionToCinematic(() => cameraDirector?.ShowDealer())
            : ShowDealerFallback(cameraDirector);

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
        yield return reactiveMusic != null
            ? reactiveMusic.TransitionToTable(() => cameraDirector?.ShowTable())
            : ShowTableFallback(cameraDirector);
        onComplete?.Invoke();
    }

    private IEnumerator ShowDealerFallback(BlackjackCameraDirector cameraDirector)
    {
        cameraDirector?.ShowDealer();
        yield return new WaitForSeconds(cameraTransitionDuration);
    }

    private IEnumerator ShowTableFallback(BlackjackCameraDirector cameraDirector)
    {
        cameraDirector?.ShowTable();
        yield return new WaitForSeconds(cameraTransitionDuration);
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
        if (voiceSource == null)
            return;

        AudioClip clipToPlay = clip != null ? clip : GetFallbackVoice();
        if (clipToPlay == null)
            return;

        voiceSource.clip = clipToPlay;
        voiceSource.Play();
    }

    private AudioClip GetFallbackVoice()
    {
        if (playerWins != null)
        {
            foreach (var opt in playerWins)
            {
                if (opt != null && opt.voice != null)
                    return opt.voice;
            }
        }
        if (dealerWins != null)
        {
            foreach (var opt in dealerWins)
            {
                if (opt != null && opt.voice != null)
                    return opt.voice;
            }
        }
        return null;
    }

    private void SetAnimation(RoundOutcome outcome)
    {
        string animation = outcome == RoundOutcome.PlayerWin || outcome == RoundOutcome.DealerBust
            ? reactingAnimation
            : talkingAnimation;
        SetAnimationName(string.IsNullOrWhiteSpace(animation) ? idleAnimation : animation);
    }

    public IEnumerator PlayGameOverSequence(BlackjackCameraDirector cameraDirector, bool isVictory, Action onComplete)
    {
        DialogueOption option = Pick(isVictory ? victoryLines : defeatLines);
        string line = option == null ? (isVictory ? "Your debt is settled in full." : "You have nothing left to give.") : option.line;

        yield return reactiveMusic != null
            ? reactiveMusic.TransitionToCinematic(() => cameraDirector?.ShowDealer())
            : ShowDealerFallback(cameraDirector);

        SetAnimationName(isVictory ? reactingAnimation : talkingAnimation);
        Show(line);
        PlayVoice(option == null ? null : option.voice);

        float readableDuration = Mathf.Max(minimumReadableDuration + 1f, line.Length / Mathf.Max(1f, charactersPerSecond));
        float voiceDuration = voiceSource != null && voiceSource.clip != null ? voiceSource.clip.length : 0f;
        yield return new WaitForSeconds(Mathf.Max(readableDuration, voiceDuration));

        if (voiceSource != null)
            voiceSource.Stop();
        Hide();
        SetAnimationName(idleAnimation);
        onComplete?.Invoke();
    }

    private void SetAnimationName(string animation)
    {
        if (dealerAnimator != null && !string.IsNullOrWhiteSpace(animation))
            dealerAnimator.Play(animation);
    }
}
