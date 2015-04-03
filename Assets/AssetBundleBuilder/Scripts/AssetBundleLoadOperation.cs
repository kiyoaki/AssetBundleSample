using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.AssetBundles
{
    public abstract class AssetBundleLoadOperation : IEnumerator
    {
        public object Current
        {
            get
            {
                return null;
            }
        }
        public bool MoveNext()
        {
            return !IsDone();
        }

        public void Reset()
        {
        }

        abstract public bool Update();

        abstract public bool IsDone();

        abstract public float GetProgress();
    }

    public class AssetBundleLoadLevelSimulationOperation : AssetBundleLoadOperation
    {
        public override bool Update()
        {
            return false;
        }

        public override bool IsDone()
        {
            return true;
        }

        public override float GetProgress()
        {
            return 1f;
        }
    }

    public class AssetBundleLoadLevelOperation : AssetBundleLoadOperation
    {
        protected string AssetBundleName;
        protected string DownloadingError;
        protected bool IsAdditive;
        protected string LevelName;
        protected AsyncOperation Request;

        public AssetBundleLoadLevelOperation(string assetbundleName, string levelName, bool isAdditive)
        {
            AssetBundleName = assetbundleName;
            LevelName = levelName;
            IsAdditive = isAdditive;
        }

        public override bool Update()
        {
            if (Request != null)
                return false;

            var bundle = AssetBundleManager.GetLoadedAssetBundle(AssetBundleName, out DownloadingError);
            if (bundle != null)
            {
                Request = IsAdditive ? Application.LoadLevelAdditiveAsync(LevelName) : Application.LoadLevelAsync(LevelName);
                return false;
            }
            return true;
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if (Request == null && DownloadingError != null)
            {
                Debug.LogError(DownloadingError);
                return true;
            }

            return Request != null && Request.isDone;
        }

        public override float GetProgress()
        {
            return Request != null ? Request.progress : 0f;
        }
    }

    public abstract class AssetBundleLoadAssetOperation : AssetBundleLoadOperation
    {
        public abstract T GetAsset<T>() where T : Object;
    }

    public class AssetBundleLoadAssetOperationSimulation : AssetBundleLoadAssetOperation
    {
        readonly Object _simulatedObject;

        public AssetBundleLoadAssetOperationSimulation(Object simulatedObject)
        {
            _simulatedObject = simulatedObject;
        }

        public override T GetAsset<T>()
        {
            return _simulatedObject as T;
        }

        public override bool Update()
        {
            return false;
        }

        public override bool IsDone()
        {
            return true;
        }

        public override float GetProgress()
        {
            return 1f;
        }
    }

    public class AssetBundleLoadAssetOperationFull : AssetBundleLoadAssetOperation
    {
        protected string AssetBundleName;
        protected string AssetName;
        protected string DownloadingError;
        protected AssetBundleRequest Request = null;
        protected Type AssetType;

        public AssetBundleLoadAssetOperationFull(string bundleName, string assetName, Type assetType)
        {
            AssetBundleName = bundleName;
            AssetName = assetName;
            AssetType = assetType;
        }

        public override T GetAsset<T>()
        {
            if (Request != null && Request.isDone)
                return Request.asset as T;
            return null;
        }

        // Returns true if more Update calls are required.
        public override bool Update()
        {
            if (Request != null)
                return false;

            var bundle = AssetBundleManager.GetLoadedAssetBundle(AssetBundleName, out DownloadingError);
            if (bundle != null)
            {
                Request = bundle.AssetBundle.LoadAssetAsync(AssetName, AssetType);
                return false;
            }
            return true;
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if (Request == null && DownloadingError != null)
            {
                Debug.LogError(DownloadingError);
                return true;
            }

            return Request != null && Request.isDone;
        }

        public override float GetProgress()
        {
            return Request != null ? Request.progress : 0f;
        }
    }

    public class AssetBundleLoadManifestOperation : AssetBundleLoadAssetOperationFull
    {
        public AssetBundleLoadManifestOperation(string bundleName, string assetName, Type assetType)
            : base(bundleName, assetName, assetType)
        {
        }

        public override bool Update()
        {
            base.Update();

            if (Request != null && Request.isDone)
            {
                AssetBundleManager.AssetBundleManifestObject = GetAsset<AssetBundleManifest>();
                return false;
            }
            return true;
        }
    }
}
