using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyStackManager : MonoBehaviour
{
    [Header("Prefab & Audio")]
    [SerializeField] private GameObject moneyPrefab;
    [SerializeField] private AudioClip dropSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Stack Positioning")]
    [SerializeField] private Vector3 baseStackPosition = new Vector3(-0.75f, 0.97f, -0.85f);
    [SerializeField] private Vector3 baseRotation = new Vector3(0f, -25f, -90f);
    [SerializeField] private float stackHeightStep = 0.045f;
    [SerializeField] private float dropHeight = 1.6f;
    [SerializeField] private float dropDuration = 0.55f;

    private readonly List<GameObject> spawnedStacks = new List<GameObject>();

    private void Awake()
    {
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

        Vector3 targetPos = baseStackPosition + new Vector3(
            Random.Range(-0.02f, 0.02f),
            stackIndex * stackHeightStep,
            Random.Range(-0.02f, 0.02f)
        );

        Quaternion targetRot = Quaternion.Euler(
            baseRotation.x,
            baseRotation.y + Random.Range(-6f, 6f),
            baseRotation.z
        );

        GameObject stackObj = Instantiate(moneyPrefab, targetPos + Vector3.up * dropHeight, targetRot);
        spawnedStacks.Add(stackObj);

        StartCoroutine(AnimateDrop(stackObj, targetPos));
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
        }
    }

    private void PlayDropSound()
    {
        if (dropSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(dropSound);
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