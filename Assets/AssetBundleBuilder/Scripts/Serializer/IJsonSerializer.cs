namespace Assets.AssetBundleBuilder
{
    public interface IJsonSerializer
    {
        string Serialize(object data);

        T Deserialize<T>(string json);
    }
}
