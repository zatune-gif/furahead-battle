using System.Runtime.InteropServices;
using UnityEngine;

public static class WebGLBridge
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern int  JS_IsMobileBrowser();
    [DllImport("__Internal")] static extern int  JS_IsFBBrowser();
    [DllImport("__Internal")] static extern int  JS_IsPortrait();
    [DllImport("__Internal")] static extern void JS_LockLandscape();
    [DllImport("__Internal")] static extern void JS_SetupMobileCanvas();
    [DllImport("__Internal")] static extern void JS_SetupAudioUnlock();
#endif

    public static bool IsMobile()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return JS_IsMobileBrowser() == 1;
#else
        return SystemInfo.deviceType == DeviceType.Handheld;
#endif
    }

    public static bool IsFBBrowser()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return JS_IsFBBrowser() == 1;
#else
        return false;
#endif
    }

    public static bool IsPortrait()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return JS_IsPortrait() == 1;
#else
        return Screen.height > Screen.width;
#endif
    }

    public static void LockLandscape()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        JS_LockLandscape();
#endif
    }

    public static void SetupMobileCanvas()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        JS_SetupMobileCanvas();
#endif
    }

    public static void SetupAudioUnlock()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        JS_SetupAudioUnlock();
#endif
    }
}
