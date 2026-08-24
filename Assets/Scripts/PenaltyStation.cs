using System.Collections;
using UnityEngine;

public class PenaltyStation : MonoBehaviour
{
    [SerializeField] private Transform hammer;
    [SerializeField] private Transform hammerRest;
    [SerializeField] private Transform hammerStrike;
    [SerializeField] private AudioSource impactAudio;
    [SerializeField] private Light impactLight;
    [SerializeField] private BlackjackCameraDirector cameraDirector;
    [SerializeField] private ReactiveMusicController music;
    [SerializeField, Min(0.1f)] private float cameraHold = 0.5f;
    [SerializeField, Min(0.1f)] private float liftDuration = 0.35f;
    [SerializeField, Min(0.05f)] private float strikeDuration = 0.16f;
    [SerializeField, Min(0.1f)] private float recoverDuration = 0.45f;
    [SerializeField, Min(0f)] private float flashDuration = 0.08f;
    [SerializeField] private int remainingFingerCount = 4;
    public int RemainingFingerCount => remainingFingerCount;

    public IEnumerator PlayLossSequence()
    {
        if (cameraDirector != null)
            cameraDirector.ShowPenalty();
        yield return new WaitForSecondsRealtime(cameraHold);
        yield return MoveHammer(hammerRest, liftDuration);
        yield return new WaitForSecondsRealtime(0.18f);
        yield return MoveHammer(hammerStrike, strikeDuration);
        if (impactAudio != null) impactAudio.Play();
        if (impactLight != null) yield return FlashImpact();
        remainingFingerCount = Mathf.Max(0, remainingFingerCount - 1);
        yield return MoveHammer(hammerRest, recoverDuration);
        if (cameraDirector != null)
            cameraDirector.ShowTable();
    }

    private IEnumerator MoveHammer(Transform target, float duration)
    {
        if (hammer == null || target == null) yield break;
        Vector3 startPosition = hammer.position;
        Quaternion startRotation = hammer.rotation;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            hammer.position = Vector3.Lerp(startPosition, target.position, t);
            hammer.rotation = Quaternion.Slerp(startRotation, target.rotation, t);
            yield return null;
        }
        hammer.SetPositionAndRotation(target.position, target.rotation);
    }

    private IEnumerator FlashImpact()
    {
        bool wasEnabled = impactLight.enabled;
        impactLight.enabled = true;
        yield return new WaitForSecondsRealtime(flashDuration);
        impactLight.enabled = wasEnabled;
    }
}
