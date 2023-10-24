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
                    uri = new Uri(uri.ToString() + "/" + fragment.TrimStart('/').TrimEnd('/'));
            }

            return uri.ToString();
        }
    }
}
