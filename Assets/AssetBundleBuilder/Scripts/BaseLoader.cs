using Assets.Scripts.AssetBundles;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class BaseLoader : MonoBehaviour
{
    // Use this for initialization.
    IEnumerator Start()
    {
        yield return StartCoroutine(Initialize());
    }

    // Initialize the downloading url and AssetBundleManifest object.
    protected IEnumerator Initialize()
    {
        // Don't destroy the game object as we base on it to run the loading script.
        DontDestroyOnLoad(gameObject);

        var request = AssetBundleManager.Initialize();
        if (request != null)
            yield return StartCoroutine(request);
    }

    protected IEnumerator Load<T>(string assetBundleName, string assetName, UnityAction<float> reportProgress = null, UnityAction<T> complete = null)
         where T : Object
    {
        Debug.Log("Start to load " + assetName + " at frame " + Time.frameCount);

        // Load asset from assetBundle.
        var request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(T));
        if (request == null)
            yield break;

        while (!request.IsDone())
        {
            if (reportProgress != null)
            {
                reportProgress(request.GetProgress());
            }
            yield return null;
        }

        if (reportProgress != null)
        {
            reportProgress(request.GetProgress());
        }

        // Get the asset.
        var prefab = request.GetAsset<T>();

        Debug.Log(assetName + (prefab == null ? " isn't" : " is") + " loaded successfully at frame " + Time.frameCount);
        if (prefab == null)
        {
            Debug.LogError("prefab is null.");
        }
        else if (complete != null)
        {
            complete(prefab);
        }
    }

    protected IEnumerator LoadLevel(string assetBundleName, string levelName, bool isAdditive)
    {
        Debug.Log("Start to load scene " + levelName + " at frame " + Time.frameCount);

        // Load level from assetBundle.
        AssetBundleLoadOperation request = AssetBundleManager.LoadLevelAsync(assetBundleName, levelName, isAdditive);
        if (request == null)
            yield break;
        yield return StartCoroutine(request);

        // This log will only be output when loading level additively.
        Debug.Log("Finish loading scene " + levelName + " at frame " + Time.frameCount);
    }

    // Update is called once per frame
    protected void Update()
    {
    }
}
