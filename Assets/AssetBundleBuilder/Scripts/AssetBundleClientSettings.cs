using Assets.Scripts.AssetBundles;
using UnityEngine;

public class AssetBundleClientSettings : ScriptableObject
{
    public string BaseUrl;

    public static string BuildAssetBundleUrl(string path)
    {
        return Instance.BaseUrl.UrlCombine(path);
    }

    #region singleton
    private static AssetBundleClientSettings _instance;
    private static readonly object LockObject = new object();

    public static AssetBundleClientSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (LockObject)
                {
                    if (_instance == null)
                    {
                        _instance = (AssetBundleClientSettings)Resources.Load(
                            "AssetBundleClientSettings");
                    }
                }
            }
            return _instance;
        }
    }
    #endregion


}