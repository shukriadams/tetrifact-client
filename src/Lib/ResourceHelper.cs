using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace TetrifactClient
{
    public class ResourceLoader
    {
        /// <summary>
        /// User Directory.Directory.Filename.Extension syntax
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static string GetFullName(string item)
        {
            return $"{Assembly.GetExecutingAssembly().GetName().Name}.{item}";
        }

        public static T DeserializeFromJson<T>(string source)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(GetFullName(source)))
            using (StreamReader reader = new StreamReader(stream))
            {
                string resource = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(resource);
            }
        }
    }
}
