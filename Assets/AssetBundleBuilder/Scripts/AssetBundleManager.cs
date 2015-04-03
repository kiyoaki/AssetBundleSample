using Assets.Scripts.AssetBundles;
using UnityEngine;
using System.Collections.Generic;

public class LoadedAssetBundle
{
    public AssetBundle AssetBundle;
    public int ReferencedCount;

    public LoadedAssetBundle(AssetBundle assetBundle)
    {
        AssetBundle = assetBundle;
        ReferencedCount = 1;
    }
}

// Class takes care of loading assetBundle and its dependencies automatically, loading variants automatically.
public class AssetBundleManager : MonoBehaviour
{
    static string[] _variants = { };
    static AssetBundleManifest _assetBundleManifest;

    static readonly Dictionary<string, LoadedAssetBundle> LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();
    static readonly Dictionary<string, WWW> DownloadingWWWs = new Dictionary<string, WWW>();
    static readonly Dictionary<string, string> DownloadingErrors = new Dictionary<string, string>();
    static readonly List<AssetBundleLoadOperation> InProgressOperations = new List<AssetBundleLoadOperation>();
    static readonly Dictionary<string, string[]> Dependencies = new Dictionary<string, string[]>();

    // The base downloading url which is used to generate the full downloading url with the assetBundle names.
    public static string BaseDownloadingUrl
    {
        get { return AssetBundleUtil.PlatformBaseUrl; }
    }

    // Variants which is used to define the active variants.
    public static string[] Variants
    {
        get { return _variants; }
        set { _variants = value; }
    }

    // AssetBundleManifest object which can be used to load the dependecies and check suitable assetBundle variants.
    public static AssetBundleManifest AssetBundleManifestObject
    {
        set { _assetBundleManifest = value; }
    }

    // Get loaded AssetBundle, only return vaild object when all the dependencies are downloaded successfully.
    static public LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, out string error)
    {
        if (DownloadingErrors.TryGetValue(assetBundleName, out error))
            return null;

        LoadedAssetBundle bundle;
        LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
        if (bundle == null)
            return null;

        // No dependencies are recorded, only the bundle itself is required.
        string[] dependencies;
        if (!Dependencies.TryGetValue(assetBundleName, out dependencies))
            return bundle;

        // Make sure all dependencies are loaded
        foreach (var dependency in dependencies)
        {
            if (DownloadingErrors.TryGetValue(assetBundleName, out error))
                return bundle;

            // Wait all the dependent assetBundles being loaded.
            LoadedAssetBundle dependentBundle;
            LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
            if (dependentBundle == null)
                return null;
        }

        return bundle;
    }

    // Load AssetBundleManifest.
    static public AssetBundleLoadManifestOperation Initialize()
    {
        var target = new GameObject("AssetBundleManager", typeof(AssetBundleManager));
        DontDestroyOnLoad(target);

        var manifestAssetBundleName = AssetBundleUtil.PlatformFolderForAssetBundles;
        LoadAssetBundle(manifestAssetBundleName, true);
        var operation = new AssetBundleLoadManifestOperation(manifestAssetBundleName, "AssetBundleManifest", typeof(AssetBundleManifest));
        InProgressOperations.Add(operation);
        return operation;
    }

    // Load AssetBundle and its dependencies.
    static protected void LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest = false)
    {
        if (!isLoadingAssetBundleManifest)
            assetBundleName = RemapVariantName(assetBundleName);

        // Check if the assetBundle has already been processed.
        bool isAlreadyProcessed = LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest);

        // Load dependencies.
        if (!isAlreadyProcessed && !isLoadingAssetBundleManifest)
            LoadDependencies(assetBundleName);
    }

    // Remaps the asset bundle name to the best fitting asset bundle variant.
    static protected string RemapVariantName(string assetBundleName)
    {
        string[] bundlesWithVariant = _assetBundleManifest.GetAllAssetBundlesWithVariant();

        // If the asset bundle doesn't have variant, simply return.
        if (System.Array.IndexOf(bundlesWithVariant, assetBundleName) < 0)
            return assetBundleName;

        string[] split = assetBundleName.Split('.');

        int bestFit = int.MaxValue;
        int bestFitIndex = -1;
        // Loop all the assetBundles with variant to find the best fit variant assetBundle.
        for (int i = 0; i < bundlesWithVariant.Length; i++)
        {
            string[] curSplit = bundlesWithVariant[i].Split('.');
            if (curSplit[0] != split[0])
                continue;

            int found = System.Array.IndexOf(_variants, curSplit[1]);
            if (found != -1 && found < bestFit)
            {
                bestFit = found;
                bestFitIndex = i;
            }
        }

        return bestFitIndex != -1 ? bundlesWithVariant[bestFitIndex] : assetBundleName;
    }

    // Where we actuall call WWW to download the assetBundle.
    static protected bool LoadAssetBundleInternal(string assetBundleName, bool isLoadingAssetBundleManifest)
    {
        // Already loaded.
        LoadedAssetBundle bundle;
        LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
        if (bundle != null)
        {
            bundle.ReferencedCount++;
            return true;
        }

        // @TODO: Do we need to consider the referenced count of WWWs?
        // In the demo, we never have duplicate WWWs as we wait LoadAssetAsync()/LoadLevelAsync() to be finished before calling another LoadAssetAsync()/LoadLevelAsync().
        // But in the real case, users can call LoadAssetAsync()/LoadLevelAsync() several times then wait them to be finished which might have duplicate WWWs.
        if (DownloadingWWWs.ContainsKey(assetBundleName))
            return true;

        var url = BaseDownloadingUrl.EndsWith("/") ?
            BaseDownloadingUrl + assetBundleName :
            BaseDownloadingUrl + "/" + assetBundleName;

        // For manifest assetbundle, always download it as we don't have hash for it.
        var download = isLoadingAssetBundleManifest ? new WWW(url) : WWW.LoadFromCacheOrDownload(url, _assetBundleManifest.GetAssetBundleHash(assetBundleName), 0);

        DownloadingWWWs.Add(assetBundleName, download);

        return false;
    }

    // Where we get all the dependencies and load them all.
    static protected void LoadDependencies(string assetBundleName)
    {
        if (_assetBundleManifest == null)
        {
            Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
            return;
        }

        // Get dependecies from the AssetBundleManifest object..
        string[] dependencies = _assetBundleManifest.GetAllDependencies(assetBundleName);
        if (dependencies.Length == 0)
            return;

        for (int i = 0; i < dependencies.Length; i++)
            dependencies[i] = RemapVariantName(dependencies[i]);

        // Record and load all dependencies.
        Dependencies.Add(assetBundleName, dependencies);
        foreach (var dependency in dependencies)
        {
            LoadAssetBundleInternal(dependency, false);
        }
    }

    // Unload assetbundle and its dependencies.
    static public void UnloadAssetBundle(string assetBundleName)
    {
        //Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + assetBundleName);

        UnloadAssetBundleInternal(assetBundleName);
        UnloadDependencies(assetBundleName);

        //Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + assetBundleName);
    }

    static protected void UnloadDependencies(string assetBundleName)
    {
        string[] dependencies;
        if (!Dependencies.TryGetValue(assetBundleName, out dependencies))
            return;

        // Loop dependencies.
        foreach (var dependency in dependencies)
        {
            UnloadAssetBundleInternal(dependency);
        }

        Dependencies.Remove(assetBundleName);
    }

    static protected void UnloadAssetBundleInternal(string assetBundleName)
    {
        string error;
        LoadedAssetBundle bundle = GetLoadedAssetBundle(assetBundleName, out error);
        if (bundle == null)
            return;

        if (--bundle.ReferencedCount == 0)
        {
            bundle.AssetBundle.Unload(false);
            LoadedAssetBundles.Remove(assetBundleName);
            //Debug.Log("AssetBundle " + assetBundleName + " has been unloaded successfully");
        }
    }

    void Update()
    {
        // Collect all the finished WWWs.
        var keysToRemove = new List<string>();
        foreach (var keyValue in DownloadingWWWs)
        {
            WWW download = keyValue.Value;

            // If downloading fails.
            if (download.error != null)
            {
                DownloadingErrors.Add(keyValue.Key, download.error);
                keysToRemove.Add(keyValue.Key);
                continue;
            }

            // If downloading succeeds.
            if (download.isDone)
            {
                //Debug.Log("Downloading " + keyValue.Key + " is done at frame " + Time.frameCount);
                LoadedAssetBundles.Add(keyValue.Key, new LoadedAssetBundle(download.assetBundle));
                keysToRemove.Add(keyValue.Key);
            }
        }

        // Remove the finished WWWs.
        foreach (var key in keysToRemove)
        {
            WWW download = DownloadingWWWs[key];
            DownloadingWWWs.Remove(key);
            download.Dispose();
        }

        // Update all in progress operations
        for (var i = 0; i < InProgressOperations.Count; )
        {
            if (!InProgressOperations[i].Update())
            {
                InProgressOperations.RemoveAt(i);
            }
            else
                i++;
        }
    }

    // Load asset from the given assetBundle.
    static public AssetBundleLoadAssetOperation LoadAssetAsync(string assetBundleName, string assetName, System.Type type)
    {
        LoadAssetBundle(assetBundleName);
        var operation = new AssetBundleLoadAssetOperationFull(assetBundleName, assetName, type);

        InProgressOperations.Add(operation);

        return operation;
    }

    // Load level from the given assetBundle.
    static public AssetBundleLoadOperation LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive)
    {
        LoadAssetBundle(assetBundleName);
        var operation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, isAdditive);

        InProgressOperations.Add(operation);

        return operation;
    }
}