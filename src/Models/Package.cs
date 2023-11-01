using System;
using System.Collections.Generic;
using TetrifactClient.Models;

namespace TetrifactClient
{
    /// <summary>
    /// Represents a downloadable package (build) on Tetrifact. This object is deserialzied directly from data
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

        /// <summary>
        /// If package is being processed locally in any way, this object contains all that stuff. This property itself is not 
        /// on tetrifact, so it is not deserialized. this object must be loaded from disk after the remote package data is fetched.
        /// </summary>
        public LocalPackage LocalPackage { get; set; }

        #endregion

        #region CTORS

        public Package()
        {
            this.Tags = new string[] { };
            this.Files = new PackageFile[0];
            this.LocalPackage = new LocalPackage();
        }

        #endregion
    }
}
