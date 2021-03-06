﻿using AssetBundleBuilder;
using UnityEditor;

[CustomEditor(typeof(AssetBundleClientSettings))]
public class AssetBundleClientSettingsEditor : SettingsEditorBase<AssetBundleClientSettings>
{
    protected const string SettingsFileName = "AssetBundleClientSettings";

    [MenuItem("AssetBundles/Show Settings")]
    public static void ShowSettings()
    {
        var settingsInstance = Load(SettingsFileName) ?? CreateNewAsset(SettingsFileName);
        if (settingsInstance != null)
        {
            Selection.activeObject = settingsInstance;
        }
    }
}