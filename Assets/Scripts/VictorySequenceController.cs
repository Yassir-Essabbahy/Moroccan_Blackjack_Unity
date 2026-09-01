using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class VictorySequenceController : MonoBehaviour
{
    [Header("Cinemachine Camera")]
    [SerializeField] private CinemachineCamera victoryCamera;

    [Header("Suitcase Objects")]
    [SerializeField] private GameObject suitcaseRoot;
    [SerializeField] private Light victorySpotlight;
    [SerializeField] private PaperMouseParallax suitcaseParallax;

    [Header("Priorities & Timing")]
    [SerializeField] private int activePriority = 60;
    [SerializeField] private int inactivePriority = 0;
    [SerializeField] private float transitionDelay = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip victoryStinger;

    public bool IsVictoryActive { get; private set; }

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (suitcaseRoot != null)
            suitcaseRoot.SetActive(false);

        if (victorySpotlight != null)
            victorySpotlight.enabled = false;
    }

    public IEnumerator PlayVictorySequence(Action onComplete = null)
    {
        IsVictoryActive = true;

        if (suitcaseRoot != null)
            suitcaseRoot.SetActive(true);

        if (victorySpotlight != null)
            victorySpotlight.enabled = true;

        if (suitcaseParallax != null)
            suitcaseParallax.isEnabled = true;

        if (victoryCamera != null)
        {
            PrioritySettings prio = victoryCamera.Priority;
            prio.Value = activePriority;
            victoryCamera.Priority = prio;
        }

        if (victoryStinger != null && audioSource != null)
        {
            audioSource.PlayOneShot(victoryStinger);
        }

        yield return new WaitForSeconds(transitionDelay);

        onComplete?.Invoke();
    }

    public void HideVictory()
    {
        IsVictoryActive = false;

        if (victoryCamera != null)
        {
            PrioritySettings prio = victoryCamera.Priority;
            prio.Value = inactivePriority;
            victoryCamera.Priority = prio;
        }

        if (suitcaseParallax != null)
            suitcaseParallax.isEnabled = false;

        if (suitcaseRoot != null)
            suitcaseRoot.SetActive(false);

        if (victorySpotlight != null)
            victorySpotlight.enabled = false;
    }
}
