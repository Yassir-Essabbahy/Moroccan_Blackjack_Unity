using UnityEngine;
using TMPro;
using System.Collections;

public class CardVisual : MonoBehaviour
{
    [SerializeField] private TextMeshPro label;

    private Card cardData;
    private bool isFaceDown;

    public void SetCard(Card card, bool faceDown = false)
    {
        cardData = card;
        isFaceDown = faceDown;

        UpdateVisual();

        if (isFaceDown)
        {
            transform.Rotate(0f, 0f, 180f); // X axis = vertical flip
        }
    }

    private void UpdateVisual()
    {
        if (label == null || cardData == null)
            return;

        label.text = cardData.Rank + "\n" + cardData.Suit;
    }

    public void Reveal()
    {
        if (!isFaceDown)
            return;

        isFaceDown = false;
        StartCoroutine(FlipAnimation());
    }

    private IEnumerator FlipAnimation()
    {
        float duration = 0.4f;
        float elapsed = 0f;

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0f, 0f, 180f); // X axis

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        transform.rotation = endRotation;
    }
}