using System.Collections;
using UnityEngine;

public class PenaltySensoryReaction : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField, Min(0f)] private float blackoutDuration = 0.16f;
    [SerializeField, Min(0.1f)] private float recoveryDuration = 3.2f;
    [Header("Audio")]
    [SerializeField] private AudioSource gameplayMusicSource;
    [SerializeField, Range(10f, 22000f)] private float muffledCutoff = 420f;
    [SerializeField, Range(10f, 22000f)] private float clearCutoff = 22000f;
    [SerializeField, Range(0f, 1f)] private float stunnedMusicVolume = 0.08f;
    [SerializeField, Range(0f, 1f)] private float ringingVolume = 0.055f;
    [SerializeField, Min(100f)] private float ringingFrequency = 5200f;
    [Header("Vision")]
    [SerializeField, Range(0f, 1f)] private float startingVisibility = 0.025f;
    [SerializeField, Range(0f, 0.2f)] private float cameraDisorientation = 0.018f;
    [SerializeField, Min(0f)] private float disorientationFrequency = 8f;

    private AudioLowPassFilter musicLowPass;
    private AudioSource ringingSource;
    private Texture2D blackTexture;
    private float blackoutAlpha;
    private float recovery;
    private bool recovering;
    private Vector3 cameraPosition;
    private bool cameraPositionCached;

    private void Awake()
    {
        if (gameplayMusicSource != null)
        {
            musicLowPass = gameplayMusicSource.GetComponent<AudioLowPassFilter>() ?? gameplayMusicSource.gameObject.AddComponent<AudioLowPassFilter>();
            musicLowPass.lowpassResonanceQ = 1f;
        }
        blackTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();
    }

    public void BeginImpactReaction()
    {
        StopAllCoroutines();
        recovering = false;
        recovery = 0f;
        blackoutAlpha = 1f;
        SetAudioState(stunnedMusicVolume, muffledCutoff, true);
        StartRinging();
        StartCoroutine(HoldBlackout());
    }

    private IEnumerator HoldBlackout()
    {
        yield return new WaitForSecondsRealtime(blackoutDuration);
        blackoutAlpha = 1f;
    }

    public IEnumerator RecoverVision()
    {
        StopAllCoroutines();
        recovering = true;
        recovery = 0f;
        blackoutAlpha = 1f;
        while (recovery < recoveryDuration)
        {
            recovery += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(recovery / recoveryDuration);
            float eased = t * t * (3f - 2f * t);
            blackoutAlpha = Mathf.Lerp(1f, 0f, eased);
            SetAudioState(Mathf.Lerp(stunnedMusicVolume, 1f, eased), Mathf.Lerp(muffledCutoff, clearCutoff, eased), true);
            yield return null;
        }
        blackoutAlpha = 0f;
        recovering = false;
        SetAudioState(1f, clearCutoff, false);
        StopRinging();
    }

    private void SetAudioState(float volumeScale, float cutoff, bool filterEnabled)
    {
        if (gameplayMusicSource != null)
            gameplayMusicSource.volume = Mathf.Clamp01(volumeScale) * 0.34f;
        if (musicLowPass != null)
        {
            musicLowPass.enabled = filterEnabled;
            musicLowPass.cutoffFrequency = cutoff;
        }
    }

    private void StartRinging()
    {
        StopRinging();
        ringingSource = gameObject.AddComponent<AudioSource>();
        ringingSource.clip = CreateRingingClip();
        ringingSource.loop = true;
        ringingSource.volume = ringingVolume;
        ringingSource.spatialBlend = 0f;
        ringingSource.playOnAwake = false;
        ringingSource.Play();
    }

    private AudioClip CreateRingingClip()
    {
        const int sampleRate = 44100;
        int sampleCount = sampleRate * 2;
        AudioClip clip = AudioClip.Create("PenaltyEarRinging", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < samples.Length; i++)
        {
            float time = i / (float)sampleRate;
            samples[i] = Mathf.Sin(2f * Mathf.PI * ringingFrequency * time) * Mathf.Lerp(0.35f, 1f, time / 2f);
        }
        clip.SetData(samples, 0);
        return clip;
    }

    private void StopRinging()
    {
        if (ringingSource == null) return;
        ringingSource.Stop();
        Destroy(ringingSource);
        ringingSource = null;
    }

    private void LateUpdate()
    {
        if (!recovering || Camera.main == null) return;
        if (!cameraPositionCached)
        {
            cameraPosition = Camera.main.transform.localPosition;
            cameraPositionCached = true;
        }
        float t = Mathf.Clamp01(recovery / recoveryDuration);
        float strength = cameraDisorientation * (1f - t);
        float time = Time.unscaledTime * disorientationFrequency;
        Camera.main.transform.localPosition = cameraPosition + new Vector3(Mathf.Sin(time) * strength, Mathf.Cos(time * 1.31f) * strength, 0f);
        Camera.main.transform.localRotation = Quaternion.Euler(Mathf.Sin(time * 0.83f) * strength * 18f, Mathf.Cos(time * 1.17f) * strength * 18f, Mathf.Sin(time * 0.61f) * strength * 12f);
    }

    private void OnGUI()
    {
        if (blackoutAlpha <= 0f || blackTexture == null) return;
        GUI.color = new Color(1f, 1f, 1f, blackoutAlpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), blackTexture);
        GUI.color = Color.white;
    }
}
