using UnityEditor;
using UnityEngine;

namespace Assets.AssetBundleBuilder
{
    public class SettingsEditorBase<T> : Editor where T : ScriptableObject
    {
        public const string RootDirectory = "Assets/";
        public const string SettingsDirectory = "Resources/";
        public const string SettingsFileExtension = ".asset";

        private T _currentSettings;

        private static string BuildAssetPath(string fileName)
        {
            return RootDirectory + SettingsDirectory + fileName + SettingsFileExtension;
        }

        public override void OnInspectorGUI()
        {
            if (target == null)
            {
                Selection.activeObject = null;
                return;
            }

            _currentSettings = (T) target;
            if (_currentSettings == null)
            {
                return;
            }

            base.OnInspectorGUI();
        }

        protected static T Load(string fileName)
        {
            return Resources.Load(fileName) as T;
        }

        protected static void CreateAsset(T asset, string fileName)
        {
            AssetDatabase.CreateAsset(asset, BuildAssetPath(fileName));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        protected static T CreateNewAsset(string fileName)
        {
            var settings = (T) CreateInstance(typeof (T));

            if (settings != null)
            {
                AssetDatabase.CreateAsset(settings, BuildAssetPath(fileName));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                return settings;
            }

            return null;
        }

        // Inspectorパネルが閉じられたときに呼ばれる（ビルド実行時にも呼ばれる）
        private void OnDisable()
        {
            if (_currentSettings != null)
            {
                EditorUtility.SetDirty(_currentSettings);
                _currentSettings = null;
            }
        }
    }
}