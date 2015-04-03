using System.Collections;
using System.Linq;
using Assets.Sample;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

public class BgmLoader : BaseLoader
{
    public AudioMixer Mixer;
    public Slider ProgressBar;
    public Text ProgressText;

    private AudioMixerGroup _bgmMixerGroup;

    // Use this for initialization
    IEnumerator Start()
    {
        _bgmMixerGroup = Mixer.FindMatchingGroups("BGM").FirstOrDefault();
        if (_bgmMixerGroup == null)
        {
            Debug.LogError("BGM MixerGroup is not found.");
            yield break;
        }

        //Initializeが完了するまで待ち
        yield return StartCoroutine(Initialize());

        //ProgressBarの最大値をダウンロードするアセット数に設定
        ProgressBar.maxValue = BgmAssetBundles.AllPath.Count();

        foreach (var assetBundle in BgmAssetBundles.AllPath.Select(x => new
        {
            //アセットのURLのパス部分
            //例）https://xxxx/assetbundle/iOS/bgm_01
            //　　上記だと「bgm_01」の部分
            Name = BgmAssetBundles.GetFileName(x),

            //読み込むアセット名
            AssetName = x
        }))
        {
            var nowAssetProgress = 0f;

            //ロードが終わるまで待ち
            yield return StartCoroutine(GetAssetBundle(assetBundle.Name, assetBundle.AssetName, x =>
            {
                var diff = x - nowAssetProgress;
                ProgressBar.value += diff;
                ProgressText.text = "ダウンロード" + (ProgressBar.value / ProgressBar.maxValue).ToString("0%");
                nowAssetProgress = x;
            }));

            AssetBundleManager.UnloadAssetBundle(assetBundle.Name);
        }
    }

    public IEnumerator GetAssetBundle(string assetBundleName, string assetName, UnityAction<float> progressNotifier)
    {
        return Load<AudioClip>(assetBundleName, assetName, progressNotifier, x =>
        {
            if (x.name == BgmAssetBundles.Bgm02)
            {
                var newGameObject = new GameObject("MainTheme");
                var audioSource = newGameObject.AddComponent<AudioSource>();
                audioSource.clip = x;
                audioSource.outputAudioMixerGroup = _bgmMixerGroup;
                audioSource.Play();
            }
        });
    }
}
