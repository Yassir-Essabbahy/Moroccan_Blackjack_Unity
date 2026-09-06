using System;
using System.Collections;
using UnityEngine;

public class CardVisual : MonoBehaviour
{
    [Header("Card")]
    [SerializeField] private MeshRenderer cardRenderer;

    [Header("Card Back")]
    [SerializeField] private Texture backTexture;

    [Header("Dealing Animation")]
    [SerializeField] private float dealDuration = 0.35f;
    [SerializeField] private float dealArcHeight = 0.2f;

    private static readonly System.Collections.Generic.Dictionary<string, Texture> CardTextureCache = 
        new System.Collections.Generic.Dictionary<string, Texture>(40);

    private Material cardMaterial;
    private bool isFaceDown;
    private Texture frontTexture;

    public bool DealFinished { get; private set; } = true;

    private void Awake()
    {
        if (cardRenderer != null)
            cardMaterial = cardRenderer.material;
    }

    private void OnDestroy()
    {
        if (cardMaterial != null)
        {
            Destroy(cardMaterial);
            cardMaterial = null;
        }
    }

    public void SetCard(Card card, bool faceDown = false)
    {
        isFaceDown = faceDown;
        frontTexture = LoadCardTexture(card);

        if (frontTexture == null)
        {
            Debug.LogError($"Could not find card artwork for: {card.Suit}_{card.Rank}");
            return;
        }

        if (cardMaterial == null && cardRenderer != null)
            cardMaterial = cardRenderer.material;

        if (faceDown)
        {
            if (cardMaterial != null) cardMaterial.mainTexture = backTexture;
            transform.Rotate(0f, 0f, 180f);
        }
        else
        {
            if (cardMaterial != null) cardMaterial.mainTexture = frontTexture;
        }
    }

    private Texture LoadCardTexture(Card card)
    {
        string fileName = $"{card.Suit}_{card.Rank}";
        if (CardTextureCache.TryGetValue(fileName, out Texture cached))
            return cached;

        Texture loaded = Resources.Load<Texture>($"CardArt/{fileName}");
        if (loaded != null)
            CardTextureCache[fileName] = loaded;
        return loaded;
    }

    private bool isFlipping;

    public void Reveal()
    {
        if (!isFaceDown || isFlipping)
            return;

        StartCoroutine(FlipRoutine());
    }

    public void DealTo(Transform targetSlot, bool revealOnArrival, float revealDelay)
    {
        DealFinished = false;
        StartCoroutine(DealRoutine(targetSlot, revealOnArrival, revealDelay));
    }

    private IEnumerator DealRoutine(Transform targetSlot, bool revealOnArrival, float revealDelay)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = targetSlot.position;
        float elapsed = 0f;

        while (elapsed < dealDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dealDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            float arc = Mathf.Sin(t * Mathf.PI) * dealArcHeight;

            transform.position = Vector3.Lerp(startPosition, endPosition, smoothT) + Vector3.up * arc;
            yield return null;
        }

        transform.position = endPosition;
        transform.SetParent(targetSlot, true);

        if (revealOnArrival)
        {
            yield return new WaitForSeconds(revealDelay);
            yield return FlipRoutine();
        }

        DealFinished = true;
    }

    private IEnumerator FlipRoutine()
    {
        if (!isFaceDown || isFlipping)
            yield break;

        isFlipping = true;
        isFaceDown = false;

        float duration = 0.4f;
        float elapsed = 0f;

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0f, 0f, 180f);
        bool textureChanged = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            // Change artwork halfway through flip
            if (!textureChanged && t >= 0.5f)
            {
                if (cardMaterial != null) cardMaterial.mainTexture = frontTexture;
                textureChanged = true;
            }

            yield return null;
        }

        transform.rotation = endRotation;
        Vector3 euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, Mathf.Round(euler.y / 90f) * 90f, 0f);

        if (cardMaterial != null) cardMaterial.mainTexture = frontTexture;
        isFlipping = false;
    }

    public void DiscardAnimation(Vector3 targetDiscardPos, float duration = 0.4f, float delay = 0f, Action onComplete = null)
    {
        StartCoroutine(DiscardRoutine(targetDiscardPos, duration, delay, onComplete));
    }

    private IEnumerator DiscardRoutine(Vector3 targetDiscardPos, float duration, float delay, Action onComplete)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0f, UnityEngine.Random.Range(-30f, 30f), 180f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            float arc = Mathf.Sin(t * Mathf.PI) * 0.15f;

            transform.position = Vector3.Lerp(startPos, targetDiscardPos, smoothT) + Vector3.up * arc;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
            yield return null;
        }

        onComplete?.Invoke();
        Destroy(gameObject);
    }
}
