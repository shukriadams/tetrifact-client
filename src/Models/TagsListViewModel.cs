using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TetrifactClient
{
    public partial class TagsListViewModel : Observable
    {
        private ObservableCollection<string> _existingTags;

        private ObservableCollection<string> _tags;

        /// <summary>
        /// List of possible existing tags, remains unchanged
        /// </summary>
        public IEnumerable<string> ExistingAll = new List<string>();

        public ObservableCollection<string> ExistingTags
        {
            get => _existingTags;
            set
            {
                _existingTags = value;
                OnPropertyChanged(nameof(ExistingTags));
            }
        }

        public ObservableCollection<string> Tags
        {
            get => _tags;
            set
            {
                _tags = value;
                OnPropertyChanged(nameof(Tags));
            }
        }

        public TagsListViewModel() 
        {
            this.ExistingTags = new ObservableCollection<string>();
            this.Tags = new ObservableCollection<string>(); 
        }
    }
}
