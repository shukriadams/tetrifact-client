using System;
using System.Collections.Generic;
using System.Linq;

namespace TetrifactClient
{
    public static class HttpHelper
    {
        public static string UrlJoin(IEnumerable<string> fragments) 
        {
            if (!fragments.Any())
                return string.Empty;

            Uri uri = null;

            foreach (string fragment in fragments)
            {
                if (uri == null)
                    uri = new Uri(fragment);
                else
                {
                    string existingUrl = uri.ToString();
                    string divider = existingUrl.EndsWith("/") ? string.Empty : "/";
                    uri = new Uri(uri.ToString() + divider + fragment.TrimStart('/').TrimEnd('/'));
                }
            }

            return uri.ToString();
        }
    }
}
