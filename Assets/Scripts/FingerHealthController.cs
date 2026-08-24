using TMPro;
using UnityEngine;

public class FingerHealthController : MonoBehaviour
{
    [SerializeField] private Animator playerHandAnimator;
    [SerializeField] private Animator dealerHandAnimator;
    [SerializeField, Range(0, 5)] private int startingFingerCount = 5;
    [SerializeField] private TMP_Text uiDisplay;
    [SerializeField] private string animatorParameter = "FingersRemaining";

    public int CurrentFingers { get; private set; }

    private void Awake()
    {
        CurrentFingers = Mathf.Clamp(startingFingerCount, 0, 5);
        ApplyState();
    }

    public void ResetForNewGame()
    {
        CurrentFingers = Mathf.Clamp(startingFingerCount, 0, 5);
        ApplyState();
    }

    public int LoseOneFinger()
    {
        CurrentFingers = Mathf.Max(0, CurrentFingers - 1);
        ApplyState();
        return CurrentFingers;
    }

    private void ApplyState()
    {
        if (playerHandAnimator != null)
            playerHandAnimator.SetInteger(animatorParameter, CurrentFingers);
        if (uiDisplay != null)
            uiDisplay.text = $"Fingers: {CurrentFingers}/5";
    }
}
