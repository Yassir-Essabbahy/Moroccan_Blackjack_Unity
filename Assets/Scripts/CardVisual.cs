using UnityEngine;
using TMPro;

public class CardVisual : MonoBehaviour
{
    [SerializeField] private TextMeshPro label;

    private Card cardData;

    public void SetCard(Card card)
    {
        cardData = card;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (label == null || cardData == null)
            return;

        label.text = cardData.Rank + "\n" + cardData.Suit;
    } 
}