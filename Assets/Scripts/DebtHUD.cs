using TMPro;
using UnityEngine;

public class DebtHUD : MonoBehaviour
{
    [Header("UI Text References")]
    [SerializeField] private TMP_Text debtText;
    [SerializeField] private TMP_Text fingersText;
    [SerializeField] private TMP_Text roundText;

    [Header("Display Formatting")]
    [SerializeField] private string currencySymbol = "DH";
    [SerializeField] private Color normalDebtColor = new Color(0.9f, 0.85f, 0.7f, 1f);
    [SerializeField] private Color clearedDebtColor = new Color(0.3f, 0.9f, 0.4f, 1f);
    [SerializeField] private Color criticalFingersColor = new Color(0.9f, 0.2f, 0.2f, 1f);

    private string normalDebtHex;
    private string clearedDebtHex;
    private string criticalFingersHex;

    private void Awake()
    {
        normalDebtHex = ColorUtility.ToHtmlStringRGB(normalDebtColor);
        clearedDebtHex = ColorUtility.ToHtmlStringRGB(clearedDebtColor);
        criticalFingersHex = ColorUtility.ToHtmlStringRGB(criticalFingersColor);
    }

    public void UpdateDisplay(int currentDebt, int currentFingers, int roundNumber)
    {
        if (debtText != null)
        {
            if (currentDebt <= 0)
            {
                debtText.text = $"DEBT: <color=#{clearedDebtHex}>PAID IN FULL</color>";
            }
            else
            {
                debtText.text = $"DEBT: <color=#{normalDebtHex}>{currentDebt:N0} {currencySymbol}</color>";
            }
        }

        if (fingersText != null)
        {
            string colorHex = currentFingers <= 1 ? criticalFingersHex : normalDebtHex;
            fingersText.text = $"FINGERS: <color=#{colorHex}>{currentFingers}/5</color>";
        }

        if (roundText != null)
        {
            roundText.text = $"ROUND {roundNumber}";
        }
    }
}