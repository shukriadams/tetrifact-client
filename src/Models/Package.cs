using System;
using System.Collections.Generic;

namespace TetrifactClient
{
    /// <summary>
    /// Represents a downloadable package (build) on Tetrifact. This object is deserialzied directly from remote.json in local packages.
    /// Is is not observable because it will not change.
    /// </summary>
    public class Package
    {
        #region PROPERTIES

        public string Id { get; set; }

        public DateTime CreatedUtc { get; set; }

        public string Hash { get; set; }

        public IEnumerable<string> Tags { get; set; }

        /// <summary>
        /// Zero for lists of package. to get size retrieve package for a specific package.
        /// </summary>
        public long Size { get; set; }

        public IEnumerable<PackageFile> Files { get; set; }

        #endregion

        #region CTORS

        public Package()
        {
            this.Tags = new string[] { };
            this.Files = new PackageFile[0];
        }

        #endregion
    }
}
