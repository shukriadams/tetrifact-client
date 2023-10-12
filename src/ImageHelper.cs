using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public static class ImageHelper
    {
        public static Bitmap LoadFromResource(Uri resourceUri)
        {
            return new Bitmap(AssetLoader.Open(resourceUri));
        }

    }
}
