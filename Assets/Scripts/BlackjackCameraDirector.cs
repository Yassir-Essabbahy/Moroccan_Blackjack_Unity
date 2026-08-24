using Unity.Cinemachine;
using UnityEngine;

public class BlackjackCameraDirector : MonoBehaviour
{
    [Header("Cinemachine Views")]
    [SerializeField] private CinemachineCamera tableOverview;
    [SerializeField] private CinemachineCamera playerFocus;
    [SerializeField] private CinemachineCamera dealerFocus;
    [Header("Priorities")]
    [SerializeField] private int inactivePriority = 10;
    [SerializeField] private int activePriority = 30;
    [Header("Dealer Beat Motion")]
    [SerializeField] private ReactiveMusicController reactiveMusic;
    [SerializeField, Min(0f)] private float intensePositionIntensity = 0.012f;
    [SerializeField, Min(0f)] private float normalPositionIntensity = 0.001f;
    [SerializeField, Min(0f)] private float intenseRotationIntensity = 0.35f;
    [SerializeField, Min(0f)] private float normalRotationIntensity = 0.03f;
    [SerializeField, Min(0f)] private float intenseFovPush = 0.8f;
    [SerializeField, Min(0f)] private float normalFovPush = 0.05f;
    [SerializeField, Range(0.1f, 20f)] private float smoothing = 7f;
    [SerializeField, Range(0f, 2f)] private float beatMultiplier = 1f;
    [SerializeField, Min(0f)] private float swayFrequency = 0.12f;
    [SerializeField, Min(0f)] private float dutchAngleOnDownbeat = 0.18f;
    [SerializeField] private bool enableDutchAngle;

    private Vector3 dealerRestPosition;
    private Quaternion dealerRestRotation;
    private float dealerRestFov;
    private float motionTime;
    private float beatPulse;
    private float targetPulse;

    private void Awake()
    {
        ResolveReferences();
        CacheDealerDefaults();
        ShowTable();
        if (reactiveMusic != null) reactiveMusic.StrongBeat += OnStrongBeat;
    }

    private void LateUpdate()
    {
        if (dealerFocus == null) return;
        float delta = Time.unscaledDeltaTime;
        motionTime += delta;
        targetPulse = Mathf.MoveTowards(targetPulse, 0f, delta * 3.5f);
        beatPulse = Mathf.Lerp(beatPulse, targetPulse, 1f - Mathf.Exp(-smoothing * delta));
        bool cinematic = reactiveMusic != null && reactiveMusic.IsCinematicActive;
        float position = cinematic ? intensePositionIntensity : normalPositionIntensity;
        float rotation = cinematic ? intenseRotationIntensity : normalRotationIntensity;
        float fov = cinematic ? intenseFovPush : normalFovPush;
        float swayX = Mathf.Sin(motionTime * swayFrequency * Mathf.PI * 2f) * position * 0.45f;
        float swayY = Mathf.Cos(motionTime * swayFrequency * Mathf.PI * 1.37f) * position * 0.25f;
        Vector3 targetPosition = dealerRestPosition + new Vector3(swayX, swayY, position * beatPulse);
        float tilt = enableDutchAngle && cinematic ? dutchAngleOnDownbeat * beatPulse : 0f;
        Quaternion targetRotation = dealerRestRotation * Quaternion.Euler(-rotation * beatPulse, 0f, tilt);
        float blend = 1f - Mathf.Exp(-smoothing * delta);
        dealerFocus.transform.localPosition = Vector3.Lerp(dealerFocus.transform.localPosition, targetPosition, blend);
        dealerFocus.transform.localRotation = Quaternion.Slerp(dealerFocus.transform.localRotation, targetRotation, blend);
        dealerFocus.Lens.FieldOfView = Mathf.Lerp(dealerFocus.Lens.FieldOfView, dealerRestFov - fov * beatPulse, blend);
    }

    public void ShowTable() => Activate(tableOverview);
    public void ShowPlayer() => Activate(playerFocus);
    public void ShowDealer() { CacheDealerDefaults(); Activate(dealerFocus); }

    private void OnStrongBeat()
    {
        if (reactiveMusic != null && reactiveMusic.IsCinematicActive)
            targetPulse = Mathf.Clamp01(targetPulse + beatMultiplier);
    }

    private void OnDestroy()
    {
        if (reactiveMusic != null) reactiveMusic.StrongBeat -= OnStrongBeat;
    }

    private void CacheDealerDefaults()
    {
        if (dealerFocus == null) return;
        dealerRestPosition = dealerFocus.transform.localPosition;
        dealerRestRotation = dealerFocus.transform.localRotation;
        dealerRestFov = dealerFocus.Lens.FieldOfView;
    }

    private void ResolveReferences()
    {
        if (tableOverview == null) tableOverview = FindCamera("CM_TableOverview");
        if (playerFocus == null) playerFocus = FindCamera("CM_PlayerFocus");
        if (dealerFocus == null) dealerFocus = FindCamera("CM_DealerFocus");
    }

    private CinemachineCamera FindCamera(string cameraName)
    {
        GameObject cameraObject = GameObject.Find(cameraName);
        return cameraObject == null ? null : cameraObject.GetComponent<CinemachineCamera>();
    }

    private void Activate(CinemachineCamera activeCamera)
    {
        if (activeCamera == null) return;
        SetPriority(tableOverview, inactivePriority);
        SetPriority(playerFocus, inactivePriority);
        SetPriority(dealerFocus, inactivePriority);
        SetPriority(activeCamera, activePriority);
        if (activeCamera != dealerFocus)
        {
            targetPulse = 0f;
            beatPulse = 0f;
            dealerFocus.transform.localPosition = dealerRestPosition;
            dealerFocus.transform.localRotation = dealerRestRotation;
            dealerFocus.Lens.FieldOfView = dealerRestFov;
        }
    }

    private static void SetPriority(CinemachineCamera camera, int value)
    {
        if (camera == null) return;
        PrioritySettings priority = camera.Priority;
        priority.Value = value;
        camera.Priority = priority;
    }
}
