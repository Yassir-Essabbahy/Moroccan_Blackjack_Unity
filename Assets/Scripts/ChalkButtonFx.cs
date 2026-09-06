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

    private float glitchTimer;
    private float currentInterval;

    private void OnEnable()
    {
        currentInterval = Random.Range(minInterval, maxInterval);
        glitchTimer = 0f;
    }

    private void Update()
    {
        if (fonts == null || fonts.Length <= 1 || label == null)
            return;

        glitchTimer += Time.deltaTime;
        if (glitchTimer >= currentInterval)
        {
            glitchTimer = 0f;
            currentInterval = Random.Range(minInterval, maxInterval);

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