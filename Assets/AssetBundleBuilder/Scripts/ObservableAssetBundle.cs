using System;
using System.Collections;
using UniRx;

namespace AssetBundleBuilder
{
    public static class ObservableAssetBundle
    {
        public static IObservable<Unit> Initialize()
        {
            return Observable.FromCoroutine(AssetBundleManager.Initialize);
        }

        public static IObservable<T> LoadAssetBundle<T>(AssetBundleStruct assetBundleStruct,
            IProgress<float> progress = null)
            where T : UnityEngine.Object
        {
            var request = AssetBundleManager.LoadAssetAsync<T>(
                assetBundleStruct.AssetBundlePath, assetBundleStruct.AssetName);

            return Observable.FromCoroutine<T>((observer, cancellation) => FetchAssetBundle(request, observer, progress, cancellation));
        }

        public static IObservable<T> LoadAssetBundle<T>(string assetBundlePath, string assetName,
            IProgress<float> progress = null)
            where T : UnityEngine.Object
        {
            return LoadAssetBundle<T>(new AssetBundleStruct
            {
                AssetBundlePath = assetBundlePath,
                AssetName = assetName
            }, progress);
        }

        private static IEnumerator FetchAssetBundle<T>(AssetBundleLoadAssetOperation operation,
            IObserver<T> observer, IProgress<float> reportProgress, CancellationToken cancel)
            where T : UnityEngine.Object
        {
            while (!operation.IsDone() && !cancel.IsCancellationRequested)
            {
                if (reportProgress != null)
                {
                    try
                    {
                        reportProgress.Report(operation.GetProgress());
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                        yield break;
                    }
                }

                yield return null;
            }

            try
            {
                if (cancel.IsCancellationRequested) yield break;

                if (reportProgress != null)
                {
                    try
                    {
                        reportProgress.Report(operation.GetProgress());
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                        yield break;
                    }
                }

                if (!string.IsNullOrEmpty(operation.GetError()))
                {
                    observer.OnError(new AssetBundleLoadAssetOperationErrorException(operation));
                }
                else
                {
                    observer.OnNext(operation.GetAsset<T>());
                    observer.OnCompleted();
                }
            }
            catch (Exception ex)
            {

                observer.OnError(ex);
            }
        }
    }

    public class AssetBundleLoadAssetOperationErrorException : Exception
    {
        public string RawErrorMessage { get; private set; }
        public AssetBundleLoadAssetOperation Operation { get; private set; }

        public AssetBundleLoadAssetOperationErrorException(AssetBundleLoadAssetOperation operation)
        {
            Operation = operation;
            RawErrorMessage = operation.GetError();
        }

        public override string ToString()
        {
            return RawErrorMessage;
        }
    }
}
