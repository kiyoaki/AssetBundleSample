using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Assets.AssetBundleBuilder
{
    public class AssetBundleBuilder
    {
        private static readonly string[] ExcludeExtensions =
    {
        ".meta", ".unity"
    };

        private static AssetBundleBuildSettings _buildSettings;
        private static string _projectRoot;
        private static string _exportFolderAbsolutePath;
        private static string _updatedFolderAbsolutePath;
        private static string _assetBundleFileName;
        private static string _assetBundleVersionFileAbsolutePath;
        private static BuildAssetBundleOptions _buildAssetBundleOptions;
        private static List<string> _updatedAssetKeyList;
        private static List<string> _noUpdatedAssetKeyList;
        private static List<AssetBundleVersion> _assetBundleVersionList;
        private static IJsonSerializer _jsonSerializer;

        private static void Init()
        {
            _buildSettings = Resources.Load<AssetBundleBuildSettings>("AssetBundleBuildSettings");

            _projectRoot = Application.dataPath.Replace("Assets", "");

            var inputFolderAbsolutePath = CombinePath(_projectRoot, _buildSettings.InputFolder);

            _exportFolderAbsolutePath = inputFolderAbsolutePath
                .Replace(_buildSettings.InputFolder, _buildSettings.ExportFolderName);

            _updatedFolderAbsolutePath = inputFolderAbsolutePath
                .Replace(_buildSettings.InputFolder, _buildSettings.UpdatedFolderName);

            _assetBundleFileName = AddAssetBundleFileExtension("assetBundle");

            const string assetBundleVersionFileName = "assetBundleVersion.txt";

            _assetBundleVersionFileAbsolutePath = CombinePath(_exportFolderAbsolutePath,
                assetBundleVersionFileName);

            if (_buildSettings.ToLowerCase)
            {
                _projectRoot = _projectRoot.ToLower();
                _exportFolderAbsolutePath = _exportFolderAbsolutePath.ToLower();
                _updatedFolderAbsolutePath = _updatedFolderAbsolutePath.ToLower();
                _assetBundleFileName = _assetBundleFileName.ToLower();
                _assetBundleVersionFileAbsolutePath = _assetBundleVersionFileAbsolutePath.ToLower();
            }

            if (_buildSettings.CollectDependencies)
            {
                _buildAssetBundleOptions |= BuildAssetBundleOptions.CollectDependencies;
            }

            if (_buildSettings.CompleteAssets)
            {
                _buildAssetBundleOptions |= BuildAssetBundleOptions.CompleteAssets;
            }

            _updatedAssetKeyList = new List<string>();
            _noUpdatedAssetKeyList = new List<string>();

            _jsonSerializer = JsonSerializerFactory.Create();

            try
            {
                _assetBundleVersionList = _jsonSerializer
                    .Deserialize<List<AssetBundleVersion>>(ReadFile(_assetBundleVersionFileAbsolutePath))
                    ?? new List<AssetBundleVersion>();
            }
            catch
            {
                _assetBundleVersionList = new List<AssetBundleVersion>();
            }
        }

        [MenuItem("Build/Export AssetBundle")]
        public static void ExportAssetBundle()
        {
            Init();

            foreach (var assetGroup in AssetDatabase.GetAllAssetPaths()
                .Where(x => x.Contains(_buildSettings.InputFolder))
                .GroupBy(path =>
                {
                    var absolutePath = CombinePath(_projectRoot, path);
                    return Path.GetDirectoryName(absolutePath);
                }))
            {
                var existsTargetFile = Directory.GetFiles(assetGroup.Key, "*.*", SearchOption.TopDirectoryOnly)
                    .Any(fileName => ExcludeExtensions.All(ext => !fileName.EndsWith(ext)));

                if (!existsTargetFile)
                {
                    continue;
                }

                var targets = assetGroup
                    .Select(path =>
                    {
                        if (path.EndsWith(".prefab"))
                        {
                            return Resources.LoadAssetAtPath<GameObject>(path);
                        }

                        var attr = File.GetAttributes(path);
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            return null;
                        }

                        return ExcludeExtensions.Any(path.EndsWith) ? null
                            : Resources.LoadAssetAtPath<Object>(path);
                    })
                    .Where(x => x != null)
                    .OrderBy(x => x.name)
                    .ToArray();

                var mainAsset = targets.FirstOrDefault();
                if (mainAsset != null)
                {
                    BuildAssetBundle(
                        mainAsset,
                        targets,
                        CombinePath(assetGroup.Key.Replace(_buildSettings.InputFolder, _buildSettings.ExportFolderName), _assetBundleFileName));
                }
            }

            var buildedAssetKeyList = _updatedAssetKeyList
                .Concat(_noUpdatedAssetKeyList)
                .ToList();

            //deleted by this build
            foreach (var assetBundleVersion in _assetBundleVersionList
                .Where(x => !buildedAssetKeyList.Contains(x.Path)).ToList())
            {
                _assetBundleVersionList.Remove(assetBundleVersion);
                var exportPath = CombinePath(_exportFolderAbsolutePath, assetBundleVersion.Path);
                DeleteFile(exportPath);
            }

            //updated by this build
            DeleteFolder(_updatedFolderAbsolutePath);
            foreach (var updatedAssetKey in _updatedAssetKeyList)
            {
                var exportPath = CombinePath(_exportFolderAbsolutePath, updatedAssetKey);
                var updatedPath = CombinePath(_updatedFolderAbsolutePath, updatedAssetKey);
                CopyFile(exportPath, updatedPath);
            }

            WriteFile(_assetBundleVersionFileAbsolutePath,
                _jsonSerializer.Serialize(_assetBundleVersionList));

            if (_updatedAssetKeyList.Count > 0)
            {
                CopyFile(_assetBundleVersionFileAbsolutePath,
                    _assetBundleVersionFileAbsolutePath.Replace(_exportFolderAbsolutePath, _updatedFolderAbsolutePath));
            }

            Debug.Log("************ Created AssetBundles ************");
        }

        private static void WriteFile(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        private static string ReadFile(string path)
        {
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        private static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static void DeleteFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private static void CopyFile(string path, string to)
        {
            if (!File.Exists(path)) return;

            CheckExistsAndCreateFolder(to);
            File.Copy(path, to);
        }

        private static string CombinePath(string path1, string path2, string delimiter = "/")
        {
            if (path1.EndsWith(delimiter))
            {
                if (path2.StartsWith(delimiter))
                {
                    return path1.Substring(0, path1.Length - 1) + path2;
                }
                return path1 + path2;
            }

            if (path2.StartsWith(delimiter))
            {
                return path1 + path2;
            }

            return path1 + delimiter + path2;
        }

        private static string AddAssetBundleFileExtension(string path)
        {
            return CombinePath(path, _buildSettings.AssetBundleFileExtension, ".");
        }

        private static void BuildAssetBundle(
            Object mainAsset,
            Object[] targetAssets,
            string outPath)
        {
            Debug.Log(string.Format("mainAsset:{0} targetAssets:{1} outPath:{2} options:{3}",
                mainAsset.name,
                string.Join(",", targetAssets.Select(x => x.name).ToArray()),
                outPath,
                _buildAssetBundleOptions));

            if (string.IsNullOrEmpty(outPath))
            {
                return;
            }

            BuildAssetBundleForTargetPlatform(mainAsset, targetAssets,
                outPath, BuildTarget.WebPlayer);

            // require iOS Pro, Android Pro Lisence

            //BuildAssetBundleForTargetPlatform(mainAsset, targetAssets,
            //    outPath, BuildTarget.Android);

            //BuildAssetBundleForTargetPlatform(mainAsset, targetAssets,
            //    outPath, BuildTarget.iOS);
        }

        private static void BuildAssetBundleForTargetPlatform(
            Object mainAsset,
            Object[] targetAssets,
            string outPath,
            BuildTarget target)
        {
            var path = outPath.Replace(_buildSettings.ExportFolderName,
                string.Format("{0}/{1}", _buildSettings.ExportFolderName, target));

            if (_buildSettings.ToLowerCase)
            {
                path = path.ToLower();
            }

            CheckExistsAndCreateFolder(path);

            uint crc;
            BuildPipeline.BuildAssetBundle(mainAsset,
                                           targetAssets,
                                           path,
                                           out crc,
                                           _buildAssetBundleOptions,
                                           target);

            var assetKey = path.Replace(CombinePath(_projectRoot, _buildSettings.ExportFolderName), "");
            var managed = _assetBundleVersionList
                .FirstOrDefault(x => x.Path == assetKey);

            if (managed == null)
            {
                _assetBundleVersionList.Add(new AssetBundleVersion
                {
                    Path = assetKey,
                    Version = 1,
                    Crc = crc
                });

                _updatedAssetKeyList.Add(assetKey);
            }
            else if (managed.Crc != crc)
            {
                managed.Version++;
                managed.Crc = crc;

                _updatedAssetKeyList.Add(assetKey);
            }
            else
            {
                _noUpdatedAssetKeyList.Add(assetKey);
            }
        }

        private static void CheckExistsAndCreateFolder(string path)
        {
            var folerPath = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(folerPath) && !Directory.Exists(folerPath))
            {
                Directory.CreateDirectory(folerPath);
            }
        }
    }
}