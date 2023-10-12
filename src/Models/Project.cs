using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrifactClient.Models
{
    public class Project
    {
        public string Name { get; set; }
        
        public string SourceServerId { get; set; }

        /// <summary>
        /// Name of Tetrifact-managed application that will be downloaded and launched. Normally ends in .exe. Lower case only
        /// </summary>
        public string ApplicationExecutableName { get; set; }

        /// <summary>
        /// Name of application process in windows. Used to track running instance of application and terminate if necessary.
        /// </summary>
        public string ApplicationProcessName { get; set; }

        public string DiffDownloadThreshold { get; set; }

        /// <summary>
        /// Can be overridden by core application value
        /// </summary>
        public int? MaxDownloadFailedAttempts { get; set; }

        /// <summary>
        /// Comma-separted tags remote packages must have to be eligable for download.
        /// </summary>
        public string RequiredTags { get; set; }

        /// <summary>
        /// Comma-separted tags remote packages will be ignored on.
        /// </summary>
        public string IgnoreTags { get; set; }

        /// <summary>
        /// Access key for tetrifact server instances that are access protected.
        /// </summary>
        public string AccessKey { get; set; }
    }
}
