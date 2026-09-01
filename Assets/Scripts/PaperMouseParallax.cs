using UnityEngine;

public class PaperMouseParallax : MonoBehaviour
{
    [Header("Tilt Settings")]
    [SerializeField] private float maxTiltAngleX = 7f;
    [SerializeField] private float maxTiltAngleY = 9f;
    [SerializeField] private float maxTiltAngleZ = 2.5f;
    [SerializeField] private float smoothSpeed = 7f;

    [Header("State")]
    public bool isEnabled = true;

    private Quaternion initialLocalRotation;

    private void Awake()
    {
        initialLocalRotation = transform.localRotation;
    }

    private void Update()
    {
        if (!isEnabled)
        {
            if (Quaternion.Angle(transform.localRotation, initialLocalRotation) > 0.05f)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, initialLocalRotation, Time.deltaTime * smoothSpeed);
            }
            return;
        }

        // Screen-center normalized cursor (-1 to 1)
        float normX = (Input.mousePosition.x / Mathf.Max(1f, Screen.width) - 0.5f) * 2f;
        float normY = (Input.mousePosition.y / Mathf.Max(1f, Screen.height) - 0.5f) * 2f;

        normX = Mathf.Clamp(normX, -1f, 1f);
        normY = Mathf.Clamp(normY, -1f, 1f);

        Quaternion targetTilt = Quaternion.Euler(
            -normY * maxTiltAngleX,
            normX * maxTiltAngleY,
            -normX * maxTiltAngleZ
        );

        Quaternion targetRotation = initialLocalRotation * targetTilt;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
    }

    public void ResetRotationImmediate()
    {
        transform.localRotation = initialLocalRotation;
    }
}