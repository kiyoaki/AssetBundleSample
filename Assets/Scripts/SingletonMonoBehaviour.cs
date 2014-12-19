using UnityEngine;

namespace Assets.AssetBundleBuilder
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = (T) FindObjectOfType(typeof (T));
                return _instance;
            }
        }

        public static bool Exists
        {
            get { return Instance != null; }
        }

        public void Awake()
        {
            if (_instance == null)
            {
                _instance = (T) this;
                return;
            }
            if (Instance == this)
            {
                return;
            }

            Destroy(this);
        }

        public void OnDestroy()
        {
            _instance = null;
        }
    }
}