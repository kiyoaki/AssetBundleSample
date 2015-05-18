using System.Collections;
using Assets.Scripts.AssetBundles;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AssetBundleSpriteImage : MonoBehaviour
{
    public string AssetBundlePath;
    public string AssetName;

    public float Progress { get; private set; }

    public bool IsDone { get; private set; }

    private Image _targetImage;

    IEnumerator Start()
    {
        _targetImage = GetComponent<Image>();

        if (string.IsNullOrEmpty(AssetBundlePath))
        {
            yield break;
        }

        if (!Caching.enabled)
        {
            yield break;
        }

        while (!Caching.ready)
        {
            yield return null;
        }

        //Initializeが完了するまで待ち
        yield return StartCoroutine(AssetBundleManager.Initialize());

        StartCoroutine(SetSprite(AssetBundlePath, AssetName));
    }

    public void ChangeSprite(string assetBundlePath, string assetName)
    {
        StartCoroutine(SetSprite(AssetBundlePath, AssetName));
    }

    IEnumerator SetSprite(string assetBundlePath, string assetName)
    {
        var request = AssetBundleManager.LoadAssetAsync(assetBundlePath, assetName, typeof(Sprite));
        if (request == null)
            yield break;

        while (!request.IsDone())
        {
            Progress = request.GetProgress();
            yield return null;
        }
        Progress = request.GetProgress();

        _targetImage.sprite = request.GetAsset<Sprite>();

        IsDone = true;

        //展開されたアセットバンドルはメモリを食うので開放
        AssetBundleManager.UnloadAssetBundle(assetBundlePath);
    }
}
