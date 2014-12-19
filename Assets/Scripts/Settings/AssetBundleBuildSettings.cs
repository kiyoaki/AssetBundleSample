using UnityEngine;

namespace Assets.AssetBundleBuilder
{
    public class AssetBundleBuildSettings : ScriptableObject
    {
        public bool ToLowerCase;

        public string InputFolder;

        public string ExportFolderName;

        public string UpdatedFolderName;

        public string AssetBundleFileExtension;

        public bool CollectDependencies;

        public bool CompleteAssets;
    }
}