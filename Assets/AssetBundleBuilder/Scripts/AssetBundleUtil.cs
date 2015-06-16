using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AssetBundleBuilder
{
    public static class AssetBundleUtil
    {
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
                // Add more build targets for your own.
                // If you add more targets, don't forget to add the same platforms to GetPlatformFolderForAssetBundles(RuntimePlatform) function.
                default:
                    return null;
            }
        }
#endif

        private static string _platformBaseUrl;

        public static string PlatformBaseUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(_platformBaseUrl))
                {
                    return _platformBaseUrl;
                }
                _platformBaseUrl = AssetBundleClientSettings.BuildAssetBundleUrl(PlatformFolderForAssetBundles);
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
    }
}
