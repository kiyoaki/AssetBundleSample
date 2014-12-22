using UnityEngine;
using UnityEngine.UI;

namespace Assets.AssetBundleBuilder
{
    public class Loading : MonoBehaviour
    {
        public float ZoomSpeed;
        public float RotateSpeed;
        public Image LoadingImage1;
        public Image LoadingImage2;

        private RectTransform _rectTransform;
        private bool _showing;
        private bool _hiding;

        void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.localScale = new Vector3(0f, 0f);
        }

        void Update()
        {
            if (_hiding)
            {
                if (_rectTransform.localScale.x > 0.01f)
                {
                    var delta = Time.deltaTime * ZoomSpeed;
                    _rectTransform.localScale -= new Vector3(delta, delta);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }

            if (_showing)
            {
                if (_rectTransform.localScale.x < 1f)
                {
                    var delta = Time.deltaTime * ZoomSpeed;
                    _rectTransform.localScale += new Vector3(delta, delta);
                }
                else
                {
                    _rectTransform.localScale = new Vector3(1f, 1f);
                    _showing = false;
                }
            }

            LoadingImage1.transform.Rotate(Vector3.back * RotateSpeed);
            LoadingImage2.transform.Rotate(Vector3.forward * RotateSpeed);
        }

        public void Show()
        {
            _showing = true;
        }

        public bool IsShowing()
        {
            return _showing;
        }

        public void Hide()
        {
            _hiding = true;
        }
    }
}