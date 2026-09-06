using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Automatically detects mobile browsers in WebGL (or mobile devices) and applies targeted optimizations:
/// - Selects the high-efficiency Mobile Quality Level (1 cascade, 12m focused shadow distance, no expensive SSAO).
/// - Caps frame rate to 60 FPS to eliminate battery drain, thermal throttling, and stutter on 90Hz/120Hz mobile screens.
/// - Sets renderScale to 0.75 on mobile, eliminating 44% of fragment shading on high-DPI retina screens.
/// - Keeps the phone screen awake during gameplay.
/// - On desktop/PC browsers, leaves full PC Quality (SSAO, 4 cascades, full fidelity) active.
/// </summary>
public static class MobilePerformanceOptimizer
{
    private static bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        if (initialized) return;
        initialized = true;

        if (IsMobileDevice())
        {
            ApplyMobileOptimizations();
        }
        else
        {
            ApplyDesktopOptimizations();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnAfterSceneLoad()
    {
        if (IsMobileDevice())
        {
            // Re-affirm renderScale on the active pipeline instance after scene assets are bound
            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp)
            {
                urp.renderScale = 0.75f;
            }
        }
    }

    private static void ApplyMobileOptimizations()
    {
        // 1. Cap to 60 FPS (prevents phone thermal-throttling on 120Hz displays)
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        // 2. Select Mobile quality tier (Quality Level 0: Mobile_RPAsset with 1 cascade, hard shadows, no SSAO)
        QualitySettings.SetQualityLevel(0, true);

        // 3. Keep phone screen awake
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // 4. Set URP render scale to 0.75 (cuts ~44% of pixel fillrate on high-DPI screens)
        if (QualitySettings.renderPipeline is UniversalRenderPipelineAsset urp)
        {
            urp.renderScale = 0.75f;
        }

        Debug.Log("[MobilePerformanceOptimizer] Mobile device detected! Applied Mobile Quality Tier (60 FPS cap, RenderScale=0.75, No SSAO, 1 Shadow Cascade).");
    }

    private static void ApplyDesktopOptimizations()
    {
        // On desktop PC: run at native refresh rate, full PC quality (SSAO, 4 shadow cascades, render scale 1.0)
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 1;
        QualitySettings.SetQualityLevel(1, true);

        if (QualitySettings.renderPipeline is UniversalRenderPipelineAsset urp)
        {
            urp.renderScale = 1.0f;
        }

        Debug.Log("[MobilePerformanceOptimizer] Desktop environment detected. Full PC Quality Profile active.");
    }

    public static bool IsMobileDevice()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            return IsMobileBrowserJS() == 1;
        }
        catch
        {
            return Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld;
        }
#else
        return Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld;
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern int IsMobileBrowserJS();
#endif
}
