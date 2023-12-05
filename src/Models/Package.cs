using System;
using System.Collections.Generic;

namespace TetrifactClient
{
    /// <summary>
    /// Represents a downloadable package (build) on Tetrifact. This object is deserialzied directly from remote.json in local packages.
    /// Is is not observable because it will not change.
    /// </summary>
    public partial class Package : Observable
    {
        #region FIELDS

        private string _id;

        private DateTime _createdUtc;

        private string _hash;

        private IEnumerable<string> _tags;
        
        private long _size;

        private IEnumerable<PackageFile> _files;

        #endregion


        #region PROPERTIES

        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public DateTime CreatedUtc
        {
            get => _createdUtc;
            set
            {
                _createdUtc = value;
                OnPropertyChanged(nameof(CreatedUtc));
            }
        }

        public string Hash
        {
            get => _hash;
            set
            {
                _hash = value;
                OnPropertyChanged(nameof(Hash));
            }
        }

        public IEnumerable<string> Tags
        {
            get => _tags;
            set
            {
                _tags = value;
                OnPropertyChanged(nameof(Tags));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public long Size
        {
            get => _size;
            set
            {
                _size = value;
                OnPropertyChanged(nameof(Size));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<PackageFile> Files
        {
            get => _files;
            set
            {
                _files = value;
                OnPropertyChanged(nameof(Files));
            }
        }

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
