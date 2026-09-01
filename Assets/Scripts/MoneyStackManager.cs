using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyStackManager : MonoBehaviour
{
    [Header("Prefab & Audio")]
    [SerializeField] private GameObject moneyPrefab;
    [SerializeField] private AudioClip dropSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Money Point Reference")]
    [SerializeField] private Transform moneyPoint;
    [SerializeField] private float tableSurfaceY = 0.94f;

    [Header("Stack Positioning")]
    [SerializeField] private Vector3 baseStackPosition = new Vector3(-1.71f, 0.94f, -1.63f);
    [SerializeField] private Vector3 baseRotation = new Vector3(0f, -25f, -90f);
    [SerializeField] private float randomYawRange = 20f;
    [SerializeField] private float stackHeightStep = 0.045f;
    [SerializeField] private float dropHeight = 1.6f;
    [SerializeField] private float dropDuration = 0.55f;

    [Header("Impact & Shake")]
    [SerializeField] private float impactShakeIntensity = 0.035f;
    [SerializeField] private float impactShakeDuration = 0.16f;

    private readonly List<GameObject> spawnedStacks = new List<GameObject>();

    private void Awake()
    {
        if (moneyPoint == null)
        {
            var mp = GameObject.Find("MoneyPoint");
            if (mp != null) moneyPoint = mp.transform;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void DropMoneyStack(int stackIndex)
    {
        if (moneyPrefab == null)
        {
            Debug.LogWarning("[MoneyStackManager] Money prefab is not assigned!");
            return;
        }

        Vector3 spawnPos;
        Vector3 targetLandingPos;

        if (moneyPoint != null)
        {
            spawnPos = moneyPoint.position;
            targetLandingPos = new Vector3(
                moneyPoint.position.x + Random.Range(-0.02f, 0.02f),
                tableSurfaceY + stackIndex * stackHeightStep,
                moneyPoint.position.z + Random.Range(-0.02f, 0.02f)
            );
        }
        else
        {
            targetLandingPos = baseStackPosition + new Vector3(
                Random.Range(-0.02f, 0.02f),
                stackIndex * stackHeightStep,
                Random.Range(-0.02f, 0.02f)
            );
            spawnPos = targetLandingPos + Vector3.up * dropHeight;
        }

        // Randomize rotation ONLY on one axis (Y-axis yaw spin)
        float randomYaw = Random.Range(-randomYawRange, randomYawRange);
        Quaternion targetRot = Quaternion.Euler(
            baseRotation.x,
            baseRotation.y + randomYaw,
            baseRotation.z
        );

        GameObject stackObj = Instantiate(moneyPrefab, spawnPos, targetRot);
        spawnedStacks.Add(stackObj);

        StartCoroutine(AnimateDrop(stackObj, targetLandingPos));
    }

    private IEnumerator AnimateDrop(GameObject stackObj, Vector3 targetPos)
    {
        if (stackObj == null)
            yield break;

        Vector3 startPos = stackObj.transform.position;
        float elapsed = 0f;

        while (elapsed < dropDuration)
        {
            if (stackObj == null)
                yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dropDuration);

            // Bounce / ease-out drop curve
            float easeT = EaseOutBounce(t);
            stackObj.transform.position = Vector3.LerpUnclamped(startPos, targetPos, easeT);
            yield return null;
        }

        if (stackObj != null)
        {
            stackObj.transform.position = targetPos;
            PlayDropSound();
            StartCoroutine(TriggerImpactMicroShake());
        }
    }

    private IEnumerator TriggerImpactMicroShake()
    {
        Transform cam = null;
        var tableCam = GameObject.Find("Cameras/CM_TableOverview");
        if (tableCam != null) cam = tableCam.transform;
        if (cam == null && Camera.main != null) cam = Camera.main.transform;
        if (cam == null) yield break;

        Vector3 originalPos = cam.localPosition;
        float elapsed = 0f;

        while (elapsed < impactShakeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / impactShakeDuration);
            float strength = impactShakeIntensity * (1f - t);

            float offsetX = Random.Range(-1f, 1f) * strength * 0.5f;
            float offsetY = -Mathf.Abs(Random.Range(0.2f, 1f)) * strength;
            float offsetZ = Random.Range(-1f, 1f) * strength * 0.3f;

            cam.localPosition = originalPos + new Vector3(offsetX, offsetY, offsetZ);
            yield return null;
        }

        cam.localPosition = originalPos;
    }

    private void PlayDropSound()
    {
        if (dropSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.82f, 0.95f);
            audioSource.PlayOneShot(dropSound, 0.9f);
        }
    }

    private float EaseOutBounce(float x)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (x < 1f / d1)
        {
            return n1 * x * x;
        }
        else if (x < 2f / d1)
        {
            x -= 1.5f / d1;
            return n1 * x * x + 0.75f;
        }
        else if (x < 2.5f / d1)
        {
            x -= 2.25f / d1;
            return n1 * x * x + 0.9375f;
        }
        else
        {
            x -= 2.625f / d1;
            return n1 * x * x + 0.984375f;
        }
    }

    public void ClearMoneyStacks()
    {
        foreach (GameObject stack in spawnedStacks)
        {
            if (stack != null)
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(stack);
                else
                    Destroy(stack);
                #else
                Destroy(stack);
                #endif
            }
        }
        spawnedStacks.Clear();
    }

    public int GetCurrentStacksCount()
    {
        return spawnedStacks.Count;
    }
}