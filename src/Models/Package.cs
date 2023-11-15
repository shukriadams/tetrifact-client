using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TetrifactClient
{
    /// <summary>
    /// Represents a downloadable package (build) on Tetrifact. This object is deserialzied directly from remote.json in local packages.
    /// Is is not observable because it will not change.
    /// </summary>
    public partial class Package : ObservableObject
    {
        #region PROPERTIES

        [property: JsonProperty("Id")]
        [ObservableProperty]
        private string _id;

        [property: JsonProperty("CreatedUtc")]
        [ObservableProperty]
        private DateTime _createdUtc;

        [property: JsonProperty("Hash")]
        [ObservableProperty]
        public string _hash;

        [property: JsonProperty("Tags")]
        [ObservableProperty]
        public IEnumerable<string> _tags;

        /// <summary>
        /// 
        /// </summary>
        [property: JsonProperty("Size")]
        [ObservableProperty]
        public long _size;

        [ObservableProperty]
        [property: Newtonsoft.Json.JsonIgnore] // need this defined twice for autogen and local 
        [JsonIgnore]
        public IEnumerable<PackageFile> _files;

        #endregion

        #region CTORS

        public Package()
        {
            this.Tags = new string[0];
            this.Files = new PackageFile[0];
        }

        #endregion
    }
}
