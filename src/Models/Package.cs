using System;
using System.Collections.Generic;

namespace TetrifactClient
{
    /// <summary>
    /// Represents a downloadable package (build) on Tetrifact
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

        public IEnumerable<string> Files { get; set; }

        #endregion

        #region CTORS

        public Package()
        {
            this.Tags = new string[] { };
        }

        #endregion
    }
}
