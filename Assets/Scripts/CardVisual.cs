using UnityEngine;
using System.Collections;

public class CardVisual : MonoBehaviour
{
    [Header("Card")]
    [SerializeField] private MeshRenderer cardRenderer;

    [Header("Card Back")]
    [SerializeField] private Texture backTexture;

    private Card cardData;
    private bool isFaceDown;

    private Texture frontTexture;

    public void SetCard(Card card, bool faceDown = false)
    {
        cardData = card;
        isFaceDown = faceDown;

        frontTexture = LoadCardTexture(card);

        if (frontTexture == null)
        {
            Debug.LogError(
                "Could not find card artwork for: " +
                card.Suit + "_" +
                card.Rank
            );

            return;
        }

        if (faceDown)
        {
            cardRenderer.material.mainTexture = backTexture;

            transform.Rotate(
                0f,
                0f,
                180f
            );
        }
        else
        {
            cardRenderer.material.mainTexture = frontTexture;
        }
    }

    private Texture LoadCardTexture(Card card)
    {
        string fileName =
            card.Suit.ToString() +
            "_" +
            card.Rank.ToString();

        return Resources.Load<Texture>(
            "CardArt/" + fileName
        );
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

        Quaternion startRotation =
            transform.rotation;

        Quaternion endRotation =
            startRotation *
            Quaternion.Euler(
                0f,
                0f,
                180f
            );

        bool textureChanged = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    endRotation,
                    t
                );

            // Change artwork halfway through flip.
            if (!textureChanged && t >= 0.5f)
            {
                cardRenderer.material.mainTexture =
                    frontTexture;

                textureChanged = true;
            }

            yield return null;
        }

        transform.rotation = endRotation;

        cardRenderer.material.mainTexture =
            frontTexture;
    }
}