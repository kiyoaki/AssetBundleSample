using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.AssetBundleBuilder
{
    public class AssetBundleDownloader : MonoBehaviour
    {
        private static Dictionary<string, float> _progressCounter = new Dictionary<string, float>();

        private const string PlayerPrefsKey = "AssetBundleVersion";

        private const string AssetBundleUrl = "https://example.com/assetbundle";

        private const string AssetBundleVersionJsonUrl =
            "https://example.com/static/assetBundleVersion.txt";

        private List<AssetBundleVersion> _versionList;
        private List<AssetBundleVersion> _newVersionList;
        private IJsonSerializer _serializer;

        // Use this for initialization
        void Start()
        {
            _progressCounter = new Dictionary<string, float>();

            _serializer = JsonSerializerFactory.Create();
            var prefs = PlayerPrefs.GetString(PlayerPrefsKey);
            if (!string.IsNullOrEmpty(prefs))
            {
                _versionList = _serializer.Deserialize<List<AssetBundleVersion>>(prefs);
            }

            StartCoroutine(DownloadVersionJson());
        }

        private bool? _isDownloading;

        private IEnumerator DownloadVersionJson()
        {
            var www = new WWW(AssetBundleVersionJsonUrl);
            while (!www.isDone)
            {
                yield return null;
            }

            if (www.error != null)
            {
                Debug.LogError(www.error);
                yield break;
            }

            _isDownloading = DownloadAssetBundles(www.text);
        }

        private const int SaveSpan = 3;
        private int _saveCounter;

        private IEnumerator DownloadAssetBundle(AssetBundleVersion version)
        {
            var www = new WWW(AssetBundleUrl + version.Path);
            _progressCounter[version.Path] = 0;
            while (!www.isDone)
            {
                _progressCounter[version.Path] = www.progress;
                yield return null;
            }

            if (www.error != null)
            {
                Debug.LogError(www.error);
                _progressCounter.Remove(version.Path);
                yield break;
            }

            _progressCounter[version.Path] = 1f;
            var assetBundle = www.assetBundle;
            Debug.Log("Loaded " + string.Join(", ", assetBundle.AllAssetNames()));
            assetBundle.Unload(true);

            version.Downloaded = true;

            //定期的にダウンロード状況を保存しておく
            _saveCounter++;
            if (_saveCounter >= SaveSpan)
            {
                _saveCounter = 0;
                PlayerPrefs.SetString(PlayerPrefsKey, _serializer.Serialize(_newVersionList));
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_isDownloading == null)
            {
                return;
            }

            if (_isDownloading.Value)
            {
                if (!Loading.Instance.IsShowing())
                {
                    Loading.Instance.Show();
                }

                var downloadPercentage = _progressCounter.Values.Sum() / _progressCounter.Count;
                Debug.Log("downloadPercentage: " + downloadPercentage);

                if (Math.Abs(_progressCounter.Values.Sum() - _progressCounter.Count) <= float.Epsilon)
                {
                    Loading.Instance.Hide();
                    Debug.Log("ダウンロード完了");
                    _isDownloading = false;
                    PlayerPrefs.SetString(PlayerPrefsKey, _serializer.Serialize(_newVersionList));
                    PlayerPrefs.Save();
                }
            }
            else
            {
                Debug.Log("ダウンロード必要なし");
                _isDownloading = null;
            }
        }

        private bool DownloadAssetBundles(string json)
        {
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            var serializer = JsonSerializerFactory.Create();
            _newVersionList = serializer.Deserialize<List<AssetBundleVersion>>(json);

            if (_newVersionList == null)
            {
                throw new Exception("newVersionList is null");
            }

            var isNeedDownload = false;
            if (_versionList == null)
            {
                //すべてダウンロード
                foreach (var newVersion in _newVersionList)
                {
                    isNeedDownload = true;
                    StartCoroutine(DownloadAssetBundle(newVersion));
                }
            }
            else
            {
                //未ダウンロードまたはVersionが更新されているものをダウンロード
                foreach (var newVersion in _newVersionList
                    .Where(newVersion =>
                    {
                        var oldVersion = _versionList.FirstOrDefault(x => x.Path == newVersion.Path);
                        return oldVersion == null || oldVersion.Version != newVersion.Version;
                    }))
                {
                    isNeedDownload = true;
                    StartCoroutine(DownloadAssetBundle(newVersion));
                }
            }

            return isNeedDownload;
        }
    }
}