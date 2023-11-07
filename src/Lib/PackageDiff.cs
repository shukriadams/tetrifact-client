using System.Collections.Generic;

namespace TetrifactClient
{
    /// <summary>
    /// A response from tetrifact describing the difference between two packages.
    /// </summary>
    public class PackageDiff
    {
        public IList<PackageFile> Difference { get; set; }

        public IList<PackageFile> Common { get; set; }
    }
}
