using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.AssetBundles
{
    public static class StringExtensions
    {
        public static string AddQueryString(this string url, string key, object value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key is null or empty");

            if (value == null)
                throw new ArgumentNullException("value");

            return url + (url.Contains("?") ? "&" : "?") + string.Format("{0}={1}", key, value);
        }

        public static string UrlCombine(this string url, string path)
        {
            return Combine(url, path, @"/");
        }

        public static string AddQueryParameter(this string url, IDictionary<string, string> parameters)
        {
            if (parameters == null || !parameters.Any())
            {
                return url;
            }

            var sb = new StringBuilder();
            foreach (var x in parameters
                .Where(x => x.Value != null))
            {
                sb.AppendFormat(url.Contains("?") ? "{0}&{1}={2}" : "{0}?{1}={2}", url, x.Key, x.Value);
            }

            return sb.ToString();
        }

        public static string PathCombine(this string url, string path)
        {
            return Combine(url, path, @"\");
        }

        private static string Combine(this string url, string path, string delimiter)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(path) || string.IsNullOrEmpty(delimiter))
            {
                return string.Empty;
            }

            if (url.EndsWith(delimiter) && path.StartsWith(delimiter))
            {
                return url + path.Substring(1);
            }

            if ((url.EndsWith(delimiter) && !path.StartsWith(delimiter))
                || (!url.EndsWith(delimiter) && path.StartsWith(delimiter)))
            {
                return url + path;
            }

            return url + delimiter + path;
        }
    }
}