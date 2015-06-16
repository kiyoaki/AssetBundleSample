using System.Linq;
using AssetBundleBuilder;
using UniRx;
using UnityEngine;

public class SpriteDownloader : MonoBehaviour
{
    public Transform ParentTransform;
    private static readonly int[] Monsters = Enumerable.Range(1, 32).ToArray();

    // Use this for initialization
    void Start()
    {
        ObservableAssetBundle.Initialize().Subscribe(_ =>
        {
            foreach (var monster in Monsters)
            {
                var obj = new GameObject("monster" + monster);
                var script = obj.AddComponent<AssetBundleSpriteImage>();
                script.AssetBundlePath = "monsters";
                script.AssetName = monster.ToString();
                obj.transform.SetParent(ParentTransform, false);
            }
        });
    }
}
