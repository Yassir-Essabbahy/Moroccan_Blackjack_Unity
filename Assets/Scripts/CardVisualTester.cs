using UnityEngine;

public class CardVisualTester : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;

    void Start()
    {
        GameObject spawnedCard = Instantiate(cardPrefab, new Vector3(0, 1, 0), Quaternion.identity);
        CardVisual visual = spawnedCard.GetComponent<CardVisual>();

        Card testCard = new Card(Suit.Suit2, Rank.Rey);
        visual.SetCard(testCard);
    }
}