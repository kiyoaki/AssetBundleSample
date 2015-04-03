namespace Assets.Sample
{
    public static class BgmAssetBundles
    {
        public const string PathPrefix = "bgm_";

        public const string Bgm01 = "01";
        public const string Bgm02 = "02";

        public static string[] AllPath
        {
            get
            {
                return new[]
                {
                    Bgm01, Bgm02
                };
            }
        }

        public static string GetFileName(string assetName)
        {
            return PathPrefix + assetName;
        }
    }
}
