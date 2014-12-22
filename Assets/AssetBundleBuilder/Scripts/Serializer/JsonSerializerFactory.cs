using LitJson;

namespace Assets.AssetBundleBuilder
{
    public class JsonSerializerFactory
    {
        public static IJsonSerializer Create()
        {
            return new LitJsonSerializer();
        }
    }
}
