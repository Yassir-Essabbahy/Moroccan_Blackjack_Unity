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
    [Header("Camera Shake & Effects")]
    [SerializeField] private Transform penaltyCameraTransform;
    [SerializeField] private float shakeIntensity = 0.18f;
    [SerializeField] private float shakeDuration = 0.65f;

    private bool sequenceRunning;

    private void Awake()
    {
        if (penaltyCameraTransform == null)
        {
            var go = GameObject.Find("Cameras/CM_PenaltyStation");
            if (go != null) penaltyCameraTransform = go.transform;
        }

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

        // Hammer Impact
        StartCoroutine(CameraShakeRoutine());
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

    private IEnumerator CameraShakeRoutine()
    {
        Transform cam = penaltyCameraTransform;
        if (cam == null)
        {
            var go = GameObject.Find("Cameras/CM_PenaltyStation");
            if (go != null) cam = go.transform;
        }

        if (cam == null) yield break;

        Vector3 originalPos = cam.localPosition;
        Quaternion originalRot = cam.localRotation;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / shakeDuration);
            float strength = shakeIntensity * Mathf.Pow(1f - t, 2f);

            float offsetX = Random.Range(-1f, 1f) * strength;
            float offsetY = Random.Range(-1f, 1f) * strength * 0.8f;
            float offsetZ = Random.Range(-1f, 1f) * strength * 0.5f;

            float rotX = Random.Range(-1f, 1f) * strength * 14f;
            float rotY = Random.Range(-1f, 1f) * strength * 14f;
            float rotZ = Random.Range(-1f, 1f) * strength * 10f;

            cam.localPosition = originalPos + new Vector3(offsetX, offsetY, offsetZ);
            cam.localRotation = originalRot * Quaternion.Euler(rotX, rotY, rotZ);

            yield return null;
        }

        cam.localPosition = originalPos;
        cam.localRotation = originalRot;
    }

    private IEnumerator FlashImpact()
    {
        bool wasEnabled = impactLight.enabled;
        impactLight.enabled = true;
        yield return new WaitForSecondsRealtime(flashDuration);
        impactLight.enabled = wasEnabled;
    }
}
