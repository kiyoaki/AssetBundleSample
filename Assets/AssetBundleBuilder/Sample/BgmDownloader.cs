using System.Linq;
using Assets.Scripts.AssetBundles;
using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UI;

public class BgmDownloader : MonoBehaviour
{
    public AudioMixer Mixer;
    public AudioSource BgmAudioSource;
    public Slider DownloadProgresSlider;
    public Text DonwloadStatusText;

    private const long RequiredDiskSpaceMegabyte = 100;
    private const float RequiredDiskSpaceByte = RequiredDiskSpaceMegabyte * 1024f * 1024f;
    private const string AssetBundlePath = "bgm/orchestra12";
    private const string AssetName = "bgm_maoudamashii_orchestra12";

    IEnumerator Start()
    {
        if (!Caching.enabled)
        {
            Debug.LogError("対応端末ではありません。");
            yield break;
        }

        if (Caching.maximumAvailableDiskSpace < RequiredDiskSpaceByte)
        {
            var needSpaceByte = RequiredDiskSpaceByte - Caching.maximumAvailableDiskSpace;
            Debug.LogError(string.Format("ディスクの空き容量が {0}MB 足りません。",
                Mathf.CeilToInt(needSpaceByte / 1024f / 1024f)));
            yield break;
        }

        while (!Caching.ready)
        {
            yield return null;
        }

        yield return StartCoroutine(AssetBundleManager.Initialize());

        var request = AssetBundleManager.LoadAssetAsync(AssetBundlePath, AssetName, typeof(Object));
        if (request == null)
            yield break;

        DownloadProgresSlider.maxValue = 1;

        while (!request.IsDone())
        {
            DownloadProgresSlider.value = request.GetProgress();
            DonwloadStatusText.text = "ダウンロード中... " + (DownloadProgresSlider.value).ToString("0%");
            yield return null;
        }

        DownloadProgresSlider.value = 1;
        DonwloadStatusText.text = "ダウンロード完了";

        BgmAudioSource.clip = request.GetAsset<AudioClip>();
        BgmAudioSource.outputAudioMixerGroup = Mixer.FindMatchingGroups("BGM").FirstOrDefault();
        BgmAudioSource.Play();

        AssetBundleManager.UnloadAssetBundle(AssetBundlePath);
    }
}
