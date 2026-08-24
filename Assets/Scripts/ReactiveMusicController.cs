using System;
using System.Collections;
using UnityEngine;

public class ReactiveMusicController : MonoBehaviour
{
    [Header("Music Clips")]
    [SerializeField] private AudioClip gameplayMusicClip;
    [SerializeField] private AudioClip dealerCinematicMusicClip;
    [Header("Audio Sources")]
    [SerializeField] private AudioSource gameplaySource;
    [SerializeField] private AudioSource dealerCinematicSource;
    [Header("BPM Sync")]
    [SerializeField, Min(0f)] private float gameplayBpm = 120f;
    [SerializeField, Min(0f)] private float gameplayBeatOffset;
    [SerializeField, Min(0f)] private float cinematicBpm = 120f;
    [SerializeField, Min(0f)] private float cinematicBeatOffset;
    [SerializeField, Range(1, 8)] private int beatsPerCameraTransition = 4;
    [SerializeField] private bool alignToBars = true;
    [Header("Levels")]
    [SerializeField, Range(0f, 1f)] private float normalVolume = 0.34f;
    [SerializeField, Range(0f, 1f)] private float cinematicVolume = 0.48f;
    [SerializeField, Min(0.1f)] private float crossfadeDuration = 1f;
    [Header("Table Music Low-Pass")]
    [SerializeField] private bool useLowPassDuringCinematic = true;
    [SerializeField, Min(10f)] private float muffledCutoffFrequency = 850f;
    [SerializeField, Min(10f)] private float clearCutoffFrequency = 22000f;
    [SerializeField, Min(0.1f)] private float lowPassResonanceQ = 1f;
    private AudioLowPassFilter gameplayLowPass;
    private Coroutine transitionRoutine;


    public event Action StrongBeat;
    public bool IsCinematicActive { get; private set; }
    private int lastStrongBeat = -1;

    private void Awake()
    {
        ConfigureSource(gameplaySource, gameplayMusicClip, normalVolume);
        ConfigureSource(dealerCinematicSource, dealerCinematicMusicClip, 0f);
        if (gameplaySource != null)
        {
            gameplayLowPass = gameplaySource.GetComponent<AudioLowPassFilter>() ?? gameplaySource.gameObject.AddComponent<AudioLowPassFilter>();
            gameplayLowPass.cutoffFrequency = clearCutoffFrequency;
            gameplayLowPass.lowpassResonanceQ = lowPassResonanceQ;
            gameplayLowPass.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (Application.isPlaying)
            PlayGameplayMusic();
    }

    private void Start()
    {
        PlayGameplayMusic();
    }

    private void PlayGameplayMusic()
    {
        if (gameplaySource == null || gameplaySource.clip == null)
        {
            Debug.LogWarning("ReactiveMusicController: gameplay music source or clip is missing.");
            return;
        }
        gameplaySource.mute = false;
        gameplaySource.volume = normalVolume;
        gameplaySource.loop = true;
        if (!gameplaySource.isPlaying)
            gameplaySource.Play();
    }

    private void Update()
    {
        if (!IsCinematicActive || dealerCinematicSource == null || cinematicBpm <= 0f)
            return;
        float beat = GetCurrentBeat(dealerCinematicSource, cinematicBpm, cinematicBeatOffset);
        int beatIndex = Mathf.FloorToInt(beat);
        if (beatIndex >= 0 && beatIndex % 4 == 0 && beatIndex != lastStrongBeat)
        {
            lastStrongBeat = beatIndex;
            StrongBeat?.Invoke();
        }
    }

    public IEnumerator TransitionToCinematic(Action onCameraStart)
    {
        yield return WaitForAlignedStart(gameplaySource, gameplayBpm, gameplayBeatOffset);
        onCameraStart?.Invoke();
        IsCinematicActive = true;
        lastStrongBeat = -1;
        BeginTransition(true);
        yield return new WaitForSecondsRealtime(GetTransitionDuration(gameplayBpm));
    }

    public IEnumerator TransitionToTable(Action onCameraStart)
    {
        yield return WaitForAlignedStart(dealerCinematicSource, cinematicBpm, cinematicBeatOffset);
        onCameraStart?.Invoke();
        IsCinematicActive = false;
        lastStrongBeat = -1;
        BeginTransition(false);
        yield return new WaitForSecondsRealtime(GetTransitionDuration(cinematicBpm));
    }

    public float GetCurrentBeat(AudioSource source, float bpm, float beatOffset)
    {
        if (source == null || !source.isPlaying || bpm <= 0f)
            return -1f;
        return (source.time - beatOffset) * bpm / 60f;
    }

    public bool IsStrongBeat(AudioSource source, float bpm, float beatOffset)
    {
        float beat = GetCurrentBeat(source, bpm, beatOffset);
        return beat >= 0f && Mathf.Abs(beat - Mathf.Round(beat)) < 0.04f && (!alignToBars || Mathf.RoundToInt(beat) % 4 == 0);
    }

    private IEnumerator WaitForAlignedStart(AudioSource source, float bpm, float offset)
    {
        if (source == null || !source.isPlaying || bpm <= 0f)
            yield break;
        float beatLength = 60f / bpm;
        float current = Mathf.Max(0f, (source.time - offset) / beatLength);
        float interval = alignToBars ? 4f : 1f;
        float next = Mathf.Ceil((current + 0.02f) / interval) * interval;
        float wait = next * beatLength - (source.time - offset);
        if (wait > 0f)
            yield return new WaitForSecondsRealtime(wait);
    }

    private float GetTransitionDuration(float bpm)
    {
        return bpm > 0f ? beatsPerCameraTransition * 60f / bpm : crossfadeDuration;
    }

    private void BeginTransition(bool cinematic)
    {
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(Crossfade(cinematic, GetTransitionDuration(cinematic ? cinematicBpm : gameplayBpm)));
    }

    private IEnumerator Crossfade(bool cinematic, float duration)
    {
        PlaySourceIfNeeded(gameplaySource);
        if (cinematic) PlaySourceIfNeeded(dealerCinematicSource);
        if (gameplayLowPass != null) gameplayLowPass.enabled = cinematic && useLowPassDuringCinematic;
        float startGameplay = gameplaySource == null ? 0f : gameplaySource.volume;
        float startCinematic = dealerCinematicSource == null ? 0f : dealerCinematicSource.volume;
        float targetGameplay = cinematic ? 0f : normalVolume;
        float targetCinematic = cinematic ? cinematicVolume : 0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (gameplaySource != null) gameplaySource.volume = Mathf.Lerp(startGameplay, targetGameplay, t);
            if (dealerCinematicSource != null) dealerCinematicSource.volume = Mathf.Lerp(startCinematic, targetCinematic, t);
            if (gameplayLowPass != null && useLowPassDuringCinematic) gameplayLowPass.cutoffFrequency = Mathf.Lerp(cinematic ? clearCutoffFrequency : muffledCutoffFrequency, cinematic ? muffledCutoffFrequency : clearCutoffFrequency, t);
            yield return null;
        }
        if (gameplaySource != null) gameplaySource.volume = targetGameplay;
        if (dealerCinematicSource != null) dealerCinematicSource.volume = targetCinematic;
        if (!cinematic && dealerCinematicSource != null) dealerCinematicSource.Pause();
        transitionRoutine = null;
    }

    private static void ConfigureSource(AudioSource source, AudioClip clip, float volume)
    {
        if (source == null) return;
        source.clip = clip; source.loop = true; source.playOnAwake = false; source.spatialBlend = 0f; source.volume = volume;
    }

    private static void PlaySourceIfNeeded(AudioSource source)
    {
        if (source != null && source.clip != null && !source.isPlaying) source.Play();
    }
}
