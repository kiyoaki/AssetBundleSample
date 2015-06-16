using System.Collections;
using System.Collections.Generic;
using AssetBundleBuilder;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AssetBundleSpriteImage : MonoBehaviour
{
    private static readonly Dictionary<Tuple<string, string>, Sprite> Loaded =
        new Dictionary<Tuple<string, string>, Sprite>();

    public string AssetBundlePath;
    public string AssetName;

    public bool IsDone { get; private set; }

    private Image _targetImage;

    void Awake()
    {
        _targetImage = GetComponent<Image>();
        _targetImage.enabled = false;
    }

    IEnumerator Start()
    {
        if (!Caching.enabled)
        {
            yield break;
        }

        while (!Caching.ready)
        {
            yield return null;
        }

        if (string.IsNullOrEmpty(AssetBundlePath))
        {
            yield break;
        }

        Sprite sprite;
        var key = Tuple.Create(AssetBundlePath, AssetName);
        if (Loaded.TryGetValue(key, out sprite))
        {
            _targetImage.sprite = sprite;
            _targetImage.enabled = true;
            IsDone = true;
            yield break;
        }

        ObservableAssetBundle.Initialize()
            .Subscribe(_ => ObservableAssetBundle.LoadAssetBundle<Sprite>(AssetBundlePath, AssetName)
                .Subscribe(x =>
                {
                    _targetImage.sprite = x;
                    _targetImage.enabled = true;
                    IsDone = true;
                    Loaded[key] = x;
                }));
    }

    void OnDestroy()
    {
        AssetBundleManager.UnloadAssetBundle(AssetBundlePath);
        Loaded.Clear();
    }
}
