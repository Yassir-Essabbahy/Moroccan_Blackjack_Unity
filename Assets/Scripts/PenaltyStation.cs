using System.Collections;
using UnityEngine;

public class PenaltyStation : MonoBehaviour
{
    [SerializeField] private Animator hammerAnimator;
    [SerializeField] private string strikeTrigger = "Strike";
    [SerializeField, Range(0f, 1f)] private float impactNormalizedTime = 0.65f;
    [SerializeField, Min(0f)] private float impactDelay;
    [SerializeField] private AudioSource impactAudio;
    [SerializeField] private Light impactLight;
    [SerializeField] private BlackjackCameraDirector cameraDirector;
    [SerializeField] private FingerHealthController fingerHealth;
    [SerializeField] private PenaltySensoryReaction sensoryReaction;
    [SerializeField, Min(0.1f)] private float cameraHold = 0.5f;
    [SerializeField, Min(0.1f)] private float strikeClipLength = 0.7f;
    [SerializeField, Min(0.1f)] private float returnClipLength = 0.5f;
    [SerializeField, Min(0f)] private float flashDuration = 0.08f;
    private bool sequenceRunning;

    private void Awake()
    {
        if (hammerAnimator == null)
            return;
        hammerAnimator.ResetTrigger(strikeTrigger);
        hammerAnimator.Play("Rest", 0, 0f);
        hammerAnimator.Update(0f);
    }

    public IEnumerator PlayLossSequence()
    {
        if (sequenceRunning)
            yield break;
        sequenceRunning = true;
        cameraDirector?.ShowPenalty();
        yield return new WaitForSecondsRealtime(cameraHold);
        if (hammerAnimator == null)
        {
            sequenceRunning = false;
            yield break;
        }
        hammerAnimator.ResetTrigger(strikeTrigger);
        hammerAnimator.SetTrigger(strikeTrigger);
        float delay = impactDelay > 0f ? impactDelay : strikeClipLength * impactNormalizedTime;
        yield return new WaitForSecondsRealtime(delay);
        sensoryReaction?.BeginImpactReaction();
        impactAudio?.Play();
        if (impactLight != null) yield return FlashImpact();
        fingerHealth?.LoseOneFinger();
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, strikeClipLength - delay) + returnClipLength);
        if (sensoryReaction != null)
            yield return sensoryReaction.RecoverVision();
        cameraDirector?.ShowTable();
        sequenceRunning = false;
    }

    private IEnumerator FlashImpact()
    {
        bool wasEnabled = impactLight.enabled;
        impactLight.enabled = true;
        yield return new WaitForSecondsRealtime(flashDuration);
        impactLight.enabled = wasEnabled;
    }
}
