using System.Collections.Generic;

namespace TetrifactClient.Models
{
    public class PackageFiles
    {
        /// <summary>
        /// Files in build. List population is context dependendent
        /// </summary>
        public IEnumerable<PackageFile> Files { get; set; }
    }
}
