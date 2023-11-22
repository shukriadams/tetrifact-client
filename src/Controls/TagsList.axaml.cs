using Avalonia.Controls;
using System.Collections.Generic;
using System.Linq;
using TetrifactClien;

namespace TetrifactClient
{
    public partial class TagsList : UserControl
    {
        public TagsChanged OnTagsChanged;

        public TagsList()
        {
            InitializeComponent();
        }

        public void SetContext(IEnumerable<string> existingTags, IEnumerable<string> boundTags) 
        {
            this.DataContext = new TagsListViewModel 
            { 
                ExistingTags = existingTags,
                Tags = boundTags.ToList()
            };
        }


        private void ExistingTags_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {

        }

        private void SelfTags_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {

        }
    }
}
