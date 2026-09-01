using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class IntroContractSequence : MonoBehaviour
{
    [Header("Game & Camera")]
    [SerializeField] private BlackjackGame game;
    [SerializeField] private CinemachineCamera introCamera;
    [SerializeField] private BlackjackCameraDirector cameraDirector;

    [Header("Contract Object")]
    [SerializeField] private GameObject contractPaper;
    [SerializeField] private PaperMouseParallax parallax;
    [SerializeField] private float slideUpDistance = 4f;
    [SerializeField] private float slideDuration = 0.85f;

    [Header("Signature UI")]
    [SerializeField] private TMP_InputField signatureInput;
    [SerializeField] private CanvasGroup signatureCanvasGroup;
    [SerializeField] private Button signButton;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip signSound;
    [SerializeField] private AudioClip slideSound;

    [Header("Settings")]
    [SerializeField] private int activePriority = 50;
    [SerializeField] private int inactivePriority = 0;
    [SerializeField] private GameObject actionPanel;

    public bool IsComplete { get; private set; }
    private bool isSigning;

    private void Awake()
    {
        if (parallax == null && contractPaper != null)
            parallax = contractPaper.GetComponent<PaperMouseParallax>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (actionPanel == null)
        {
            var ap = GameObject.Find("Canvas/ActionPanel");
            if (ap != null) actionPanel = ap;
        }

        if (signButton != null)
            signButton.onClick.AddListener(ConfirmSignature);

        if (signatureInput != null)
        {
            signatureInput.onSubmit.AddListener((val) => ConfirmSignature());
        }
    }

    private void Start()
    {
        // Check if intro should play
        if (!IsComplete)
        {
            StartIntro();
        }
    }

    private void Update()
    {
        if (IsComplete || isSigning)
            return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ConfirmSignature();
        }
    }

    public void StartIntro()
    {
        IsComplete = false;
        isSigning = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (actionPanel != null)
            actionPanel.SetActive(false);

        if (introCamera != null)
        {
            PrioritySettings prio = introCamera.Priority;
            prio.Value = activePriority;
            introCamera.Priority = prio;
        }

        if (contractPaper != null)
            contractPaper.SetActive(true);

        if (parallax != null)
            parallax.isEnabled = true;

        if (signatureCanvasGroup != null)
        {
            signatureCanvasGroup.alpha = 1f;
            signatureCanvasGroup.interactable = true;
            signatureCanvasGroup.blocksRaycasts = true;
        }

        if (signatureInput != null)
        {
            signatureInput.keyboardType = TouchScreenKeyboardType.Default;
            #if !UNITY_ANDROID && !UNITY_IOS
            signatureInput.ActivateInputField();
            signatureInput.Select();
            #endif
        }
    }

    public void ConfirmSignature()
    {
        if (isSigning || IsComplete)
            return;

        string playerName = signatureInput != null && !string.IsNullOrWhiteSpace(signatureInput.text)
            ? signatureInput.text.Trim()
            : "Borrower";

        PlayerPrefs.SetString("PlayerName", playerName);
        Debug.Log($"[IntroContractSequence] Contract signed by: {playerName}");

        isSigning = true;
        if (parallax != null)
            parallax.isEnabled = false;

        if (signatureCanvasGroup != null)
        {
            signatureCanvasGroup.interactable = false;
            signatureCanvasGroup.blocksRaycasts = false;
        }

        StartCoroutine(SlideUpAndTransitionRoutine());
    }

    private IEnumerator SlideUpAndTransitionRoutine()
    {
        if (signSound != null && audioSource != null)
            audioSource.PlayOneShot(signSound);

        yield return new WaitForSeconds(0.2f);

        if (slideSound != null && audioSource != null)
            audioSource.PlayOneShot(slideSound);

        // Animate paper sliding upwards
        if (contractPaper != null)
        {
            Vector3 startPos = contractPaper.transform.position;
            Vector3 targetPos = startPos + Vector3.up * slideUpDistance;
            Quaternion startRot = contractPaper.transform.rotation;
            Quaternion targetRot = startRot * Quaternion.Euler(-15f, 0f, 0f);
            float elapsed = 0f;

            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / slideDuration);
                float ease = Mathf.SmoothStep(0f, 1f, t);

                contractPaper.transform.position = Vector3.Lerp(startPos, targetPos, ease);
                contractPaper.transform.rotation = Quaternion.Slerp(startRot, targetRot, ease);

                if (signatureCanvasGroup != null)
                    signatureCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t * 2f);

                yield return null;
            }

            contractPaper.SetActive(false);
        }

        // Lower camera priority so Cinemachine smoothly blends down to table overview
        if (introCamera != null)
        {
            PrioritySettings prio = introCamera.Priority;
            prio.Value = inactivePriority;
            introCamera.Priority = prio;
        }

        if (cameraDirector != null)
        {
            cameraDirector.ShowTable();
        }

        // Wait for camera blend
        yield return new WaitForSeconds(1.0f);

        IsComplete = true;
        isSigning = false;

        if (actionPanel != null)
            actionPanel.SetActive(true);

        // Begin the match!
        if (game != null)
        {
            game.StartRound();
        }
    }
}