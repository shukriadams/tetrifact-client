using Avalonia.Controls;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using TetrifactClien;
using TetrifactClient.Lib;

namespace TetrifactClient
{
    public partial class TagsList : UserControl
    {
        private IEnumerable<string> _tags = new string[0];

        public TagsChanged OnTagsChanged;

        public IEnumerable<string> Tags
        {
            get 
            { 
                return _tags;
            } 
        }

        public TagsList()
        {
            TagsListViewModel context = new TagsListViewModel();
            this.DataContext = context;
            InitializeComponent();
        }

        public void SetContext(IEnumerable<string> existingTags, IEnumerable<string> boundTags) 
        {
            TagsListViewModel context = this.DataContext as TagsListViewModel;

            foreach (string tag in existingTags)
                context.ExistingTags.Add(tag);

            foreach (string tag in boundTags)
                context.Tags.Add(tag);

            panelExistingTags.IsVisible = context.ExistingTags.Any();

            context.Tags.CollectionChanged += (object e, NotifyCollectionChangedEventArgs args) =>{
                panelCurrentTags.IsVisible = context.Tags.Any();
            };
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

            context.Tags.Add(source.Text);
            _tags = context.Tags;
        }

        private void SelfTags_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            Prompt prompt = new Prompt();
            Window owner = this.GetOwnerWindow();
            prompt.ShowDialog(owner);
            prompt.CenterOn(owner);

            prompt.OnAccept =()=>{
                TextBlock source = e.Source as TextBlock;
                if (source == null)
                    return;

                TagsListViewModel context = this.DataContext as TagsListViewModel;
                if (context == null)
                    return;

                context.Tags.Remove(source.Text);
                _tags = context.Tags;
            };
        }

        private void AddNew_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtTag.Text))
                return;
            
            TagsListViewModel context = this.DataContext as TagsListViewModel;
            if (context == null)
                return;

            if (context.Tags.Any(t => t == txtTag.Text))
                return;

            context.Tags.Add(txtTag.Text);
            txtTag.Text = string.Empty;
            _tags = context.Tags;
        }
    }
}
