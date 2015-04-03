using System.IO;
using UnityEditor;

public class BuildScript
{
    const string AssetBundlesOutputPath = "AssetBundles";
    private static readonly BuildTarget[] BuildTargets =
    {
        BuildTarget.iOS, BuildTarget.Android
    };

    public static void BuildAssetBundles()
    {
        foreach (var buildTarget in BuildTargets)
        {
            var outputPath = Path.Combine(AssetBundlesOutputPath,
                AssetBundleUtil.GetPlatformFolderForAssetBundles(buildTarget));

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            BuildPipeline.BuildAssetBundles(outputPath,
                BuildAssetBundleOptions.None, buildTarget);
        }
    }
}