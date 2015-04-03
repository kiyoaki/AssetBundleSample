using UnityEditor;

[CustomEditor(typeof(AssetBundleClientSettings))]
public class AssetBundleBuildSettingsEditor : SettingsEditorBase<AssetBundleClientSettings>
{
    protected const string SettingsFileName = "AssetBundleClientSettings";

    [MenuItem("Edit/Show AssetBundle Build Settings")]
    public static void ShowSettings()
    {
        var settingsInstance = Load(SettingsFileName) ?? CreateNewAsset(SettingsFileName);
        if (settingsInstance != null)
        {
            Selection.activeObject = settingsInstance;
        }
    }
}