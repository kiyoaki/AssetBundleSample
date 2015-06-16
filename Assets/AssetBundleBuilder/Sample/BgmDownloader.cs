using System.Collections;
using System.Linq;
using AssetBundleBuilder;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class BgmDownloader : MonoBehaviour, IProgress<float>
{
    public AudioMixer Mixer;
    public AudioSource BgmAudioSource;
    public Slider DownloadProgresSlider;
    public Text DonwloadStatusText;

    private const long RequiredDiskSpaceMegabyte = 100;
    private const long RequiredDiskSpaceByte = RequiredDiskSpaceMegabyte * 1024 * 1024;
    private const string AssetBundlePath = "bgm/orchestra12";
    private const string AssetName = "bgm_maoudamashii_orchestra12";

    IEnumerator Start()
    {
        if (!Caching.enabled)
        {
            Debug.LogError("対応端末ではありません。");
            yield break;
        }

        while (!Caching.ready)
        {
            yield return null;
        }

        Caching.maximumAvailableDiskSpace = RequiredDiskSpaceByte;
        DownloadProgresSlider.maxValue = 1;

        ObservableAssetBundle.Initialize()
            .Subscribe(_ => ObservableAssetBundle.LoadAssetBundle<AudioClip>(AssetBundlePath, AssetName, this)
                .Subscribe(clip =>
                {
                    DownloadProgresSlider.value = 1;
                    DonwloadStatusText.text = "BGMダウンロード完了";

                    BgmAudioSource.clip = clip;
                    BgmAudioSource.outputAudioMixerGroup = Mixer.FindMatchingGroups("BGM").FirstOrDefault();
                    BgmAudioSource.Play();

                    AssetBundleManager.UnloadAssetBundle(AssetBundlePath);
                }));
    }

    public void Report(float value)
    {
        DownloadProgresSlider.value = value;
        DonwloadStatusText.text = "BGMダウンロード中... " + value.ToString("0%");
    }
}
