using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace TetrifactClient
{
    public partial class TagsListViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<string> _existingTags;

        [ObservableProperty]
        private ObservableCollection<string> _tags;

        public TagsListViewModel() 
        {
            this.ExistingTags = new ObservableCollection<string>();
            this.Tags = new ObservableCollection<string>(); 
        }
    }
}
