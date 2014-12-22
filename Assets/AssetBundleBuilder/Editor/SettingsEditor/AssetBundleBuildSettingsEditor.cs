using UnityEditor;

namespace Assets.AssetBundleBuilder
{
    [CustomEditor(typeof(AssetBundleBuildSettings))]
    public class AssetBundleBuildSettingsEditor : SettingsEditorBase<AssetBundleBuildSettings>
    {
        protected const string SettingsFileName = "AssetBundleBuildSettings";

        [MenuItem("Edit/Show AssetBundle Build Settings")]
        public static void ShowSettings()
        {
            var settingsInstance = Load(SettingsFileName) ?? CreateNewAsset(SettingsFileName);
            if (settingsInstance != null)
            {
                Selection.activeObject = settingsInstance;
            }
        }

        [MenuItem("Edit/Set Default AssetBundle Build Settings")]
        public static void CreateDefaultSettings()
        {
            var settingsInstance = Load(SettingsFileName) ?? CreateNewAsset(SettingsFileName);
            if (settingsInstance != null)
            {
                settingsInstance.AssetBundleFileExtension = ".unity3d";
                settingsInstance.InputFolder = "Assets/AssetBundle/Resources";
                settingsInstance.ExportFolderName = "Export";
                settingsInstance.UpdatedFolderName = "Updated";
                settingsInstance.ToLowerCase = false;
                settingsInstance.CollectDependencies = true;
                settingsInstance.CompleteAssets = true;

                Selection.activeObject = settingsInstance;
            }
        }
    }
}
