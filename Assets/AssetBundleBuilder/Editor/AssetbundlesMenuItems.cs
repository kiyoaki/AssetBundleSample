using UnityEditor;

public class AssetbundlesMenuItems
{
    [MenuItem("AssetBundles/Build AssetBundles")]
    public static void BuildAssetBundles()
    {
        BuildScript.BuildAssetBundles();
    }
}
