using System.IO;
using AssetBundleBuilder;
using UnityEditor;

public class BuildScript
{
    const string AssetBundlesOutputPath = "AssetBundles";

    [MenuItem("AssetBundles/Build for iOS")]
    public static void BuildIosAssetBundles()
    {
        BuildAssetBundles(BuildTarget.iOS);
    }

    [MenuItem("AssetBundles/Build for Android")]
    public static void BuildAndroidAssetBundles()
    {
        BuildAssetBundles(BuildTarget.Android);
    }

    [MenuItem("AssetBundles/Build for iOS and Android")]
    public static void BuildIosAndroidAssetBundles()
    {
        foreach (var buildTarget in new[] { BuildTarget.iOS, BuildTarget.Android })
        {
            BuildAssetBundles(buildTarget);
        }
    }

    private static void BuildAssetBundles(BuildTarget buildTarget)
    {
        var outputPath = Path.Combine(AssetBundlesOutputPath,
            AssetBundleUtil.GetPlatformFolderForAssetBundles(buildTarget));

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        const BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle |
                                                BuildAssetBundleOptions.ForceRebuildAssetBundle;

        BuildPipeline.BuildAssetBundles(outputPath, options, buildTarget);
    }
}