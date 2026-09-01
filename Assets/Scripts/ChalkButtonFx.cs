using System.Collections;
using UnityEngine;
using TMPro;

public class ChalkButtonFX : MonoBehaviour
{
    [Header("Fonts")]
    [SerializeField] private TMP_FontAsset[] fonts = new TMP_FontAsset[4];
    [SerializeField] private TextMeshProUGUI label;

    [Header("Glitch Speed")]
    [SerializeField] private float minInterval = 0.05f;
    [SerializeField] private float maxInterval = 0.15f;

    private int currentFontIndex = 0;

    private void Awake()
    {
        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>();

        if (fonts != null && fonts.Length > 0)
            ApplyFont(0);
    }

    private void OnEnable()
    {
        StartCoroutine(FontGlitchLoop());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator FontGlitchLoop()
    {
        if (fonts == null || fonts.Length <= 1 || label == null)
            yield break;

        while (true)
        {
            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);

            int nextFont;

            do
            {
                nextFont = Random.Range(0, fonts.Length);
            }
            while (nextFont == currentFontIndex);

            currentFontIndex = nextFont;
            ApplyFont(currentFontIndex);
        }
    }

    private void ApplyFont(int index)
    {
        if (label == null || fonts == null)
            return;

        if (index < 0 || index >= fonts.Length)
            return;

        if (fonts[index] == null)
            return;

        label.font = fonts[index];
    }

    public void CycleFont()
    {
        if (fonts == null || fonts.Length == 0)
            return;

        currentFontIndex++;
        currentFontIndex %= fonts.Length;

        ApplyFont(currentFontIndex);
    }
}