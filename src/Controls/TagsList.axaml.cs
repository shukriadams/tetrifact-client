using Avalonia.Controls;
using System.Collections.Concurrent;
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
            TagsListViewModel context = new TagsListViewModel();
            this.DataContext = context;
            InitializeComponent();
        }

        public void SetContext(IEnumerable<string> existingTags, IEnumerable<string> boundTags) 
        {
            TagsListViewModel context = this.DataContext as TagsListViewModel;
            foreach(string tag in existingTags)
                context.ExistingTags.Add(tag);

            foreach (string tag in boundTags)
                context.Tags.Add(tag);
        }

        private void ExistingTags_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            TextBlock source = e.Source as TextBlock;
            if (source == null)
                return;

            TagsListViewModel context = this.DataContext as TagsListViewModel;
            if (context == null)
                return;

            if (context.Tags.Any(t => t == source.Text))
                return;

            context.Tags.Append(source.Text);
        }

        private void SelfTags_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            TextBlock source = e.Source as TextBlock;
            if (source == null)
                return;

            string test = "";
        }
    }
}
