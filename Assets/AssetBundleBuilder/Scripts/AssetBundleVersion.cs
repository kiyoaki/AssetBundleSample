using System;

namespace Assets.AssetBundleBuilder
{
    public class AssetBundleVersion : IEquatable<AssetBundleVersion>
    {
        public string Path;
        public int Version;
        public uint Crc;
        public bool Downloaded;

        public AssetBundleVersion()
        {
            Version = 1;
        }

        public bool Equals(AssetBundleVersion other)
        {
            if (other == null)
            {
                return false;
            }
            return Path == other.Path;
        }

        public override string ToString()
        {
            return string.Join(", ", new[]
            {
                string.Format("Path: {0}", Path),
                string.Format("Version: {0}", Version),
                string.Format("Crc: {0}", Crc),
            });
        }
    }
}