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

    private void Awake()
    {
        ResolveReferences();
        ShowTable();
    }

    public void ShowTable() => Activate(tableOverview);

    public void ShowPlayer() => Activate(playerFocus);

    public void ShowDealer() => Activate(dealerFocus);

    private void ResolveReferences()
    {
        if (tableOverview == null)
            tableOverview = FindCamera("CM_TableOverview");
        if (playerFocus == null)
            playerFocus = FindCamera("CM_PlayerFocus");
        if (dealerFocus == null)
            dealerFocus = FindCamera("CM_DealerFocus");
    }

    private CinemachineCamera FindCamera(string cameraName)
    {
        GameObject cameraObject = GameObject.Find(cameraName);
        return cameraObject == null
            ? null
            : cameraObject.GetComponent<CinemachineCamera>();
    }

    private void Activate(CinemachineCamera activeCamera)
    {
        if (activeCamera == null)
        {
            Debug.LogWarning("Cinemachine camera reference is missing.");
            return;
        }

        SetPriority(tableOverview, inactivePriority);
        SetPriority(playerFocus, inactivePriority);
        SetPriority(dealerFocus, inactivePriority);
        SetPriority(activeCamera, activePriority);
    }

    private static void SetPriority(CinemachineCamera camera, int value)
    {
        if (camera == null)
            return;

        PrioritySettings priority = camera.Priority;
        priority.Value = value;
        camera.Priority = priority;
    }
}
