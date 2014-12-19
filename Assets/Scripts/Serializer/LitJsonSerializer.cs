using LitJson;

namespace Assets.AssetBundleBuilder
{
    internal class LitJsonSerializer : IJsonSerializer
    {
        public string Serialize(object data)
        {
            return JsonMapper.ToJson(data);
        }

        public T Deserialize<T>(string json)
        {
            return JsonMapper.ToObject<T>(json);
        }
    }
}
