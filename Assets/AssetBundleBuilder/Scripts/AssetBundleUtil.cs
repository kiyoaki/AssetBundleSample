using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class AssetBundleUtil
{
    private static string _platformBaseUrl;

    public static string PlatformBaseUrl
    {
        get
        {
            if (!string.IsNullOrEmpty(_platformBaseUrl)) return _platformBaseUrl;

            var buildSettings = Resources.Load<AssetBundleClientSettings>("AssetBundleClientSettings");
            if (buildSettings == null || string.IsNullOrEmpty(buildSettings.BaseUrl))
            {
                Debug.LogError("AssetBundleClientSettings BaseUrl is required.");
                return "";
            }

            _platformBaseUrl = buildSettings.BaseUrl.EndsWith("/") ?
                string.Format(buildSettings.BaseUrl + PlatformFolderForAssetBundles) :
                string.Format(buildSettings.BaseUrl + "/" + PlatformFolderForAssetBundles);

            return _platformBaseUrl;
        }
    }

    private static string _platformFolderForAssetBundles;

    public static string PlatformFolderForAssetBundles
    {
        get
        {
            if (!string.IsNullOrEmpty(_platformFolderForAssetBundles)) return _platformFolderForAssetBundles;
#if UNITY_EDITOR
            _platformFolderForAssetBundles =
                GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
                _platformFolderForAssetBundles = GetPlatformFolderForAssetBundles(Application.platform);
#endif
            return _platformFolderForAssetBundles;
        }
    }

    public static string GetPlatformFolderForAssetBundles(RuntimePlatform platform)
    {
        switch (platform)
        {
            case RuntimePlatform.Android:
                return "Android";
            case RuntimePlatform.IPhonePlayer:
                return "iOS";
            case RuntimePlatform.WindowsWebPlayer:
            case RuntimePlatform.OSXWebPlayer:
                return "WebPlayer";
            case RuntimePlatform.WindowsPlayer:
                return "Windows";
            case RuntimePlatform.OSXPlayer:
                return "OSX";
            // Add more build platform for your own.
            // If you add more platforms, don't forget to add the same targets to GetPlatformFolderForAssetBundles(BuildTarget) function.
            default:
                return null;
        }
    }

#if UNITY_EDITOR
    public static string GetPlatformFolderForAssetBundles(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "iOS";
            case BuildTarget.WebPlayer:
                return "WebPlayer";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Windows";
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSXUniversal:
                return "OSX";
            default:
                return null;
        }
    }
#endif
}
