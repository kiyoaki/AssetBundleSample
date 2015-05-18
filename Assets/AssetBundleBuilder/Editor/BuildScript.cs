using System.IO;
using Assets.Scripts.AssetBundles;
using UnityEditor;

public class BuildScript
{
    const string AssetBundlesOutputPath = "AssetBundles";

    private static readonly BuildTarget[] BuildTargets =
    {
        BuildTarget.iOS, BuildTarget.Android
    };

    private const BuildAssetBundleOptions BuildOptions = 
        BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle;

    public static void BuildAssetBundles()
    {
        foreach (var buildTarget in BuildTargets)
        {
            var outputPath = Path.Combine(AssetBundlesOutputPath,
                AssetBundleUtil.GetPlatformFolderForAssetBundles(buildTarget));

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            BuildPipeline.BuildAssetBundles(outputPath, BuildOptions, buildTarget);
        }
    }
}