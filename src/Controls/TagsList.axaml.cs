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

            // add demo tags
            context.ExistingTags.Add("test1");
            context.ExistingTags.Add("test2");
            context.Tags.Add("test1");
            context.Tags.Add("test1");
        }

        public void SetContext(IEnumerable<string> existingTags, IEnumerable<string> boundTags) 
        {
            TagsListViewModel context = this.DataContext as TagsListViewModel;

            // clear demo tags
            context.ExistingTags.Clear();
            context.Tags.Clear();

            foreach (string tag in existingTags)
                context.ExistingTags.Add(tag);

            foreach (string tag in boundTags)
                context.Tags.Add(tag);

            panelExistingTags.IsVisible = context.ExistingTags.Any();

            context.Tags.CollectionChanged += (object e, NotifyCollectionChangedEventArgs args) =>{
                SetVisiblity();
            };

            SetVisiblity();
        }

        private void SetVisiblity() 
        {
            TagsListViewModel context = this.DataContext as TagsListViewModel;
            panelCurrentTags.IsVisible = context.Tags.Any();
        }


        private void ExistingTags_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            string tag = GetTagTextFromEvent(e);
            if (tag == null)
                return;

            TagsListViewModel context = this.DataContext as TagsListViewModel;
            if (context == null)
                return;

            if (context.Tags.Any(t => t == tag))
                return;

            context.Tags.Add(tag);
            _tags = context.Tags;
        }

        private static string GetTagTextFromEvent(Avalonia.Input.TappedEventArgs e) 
        {
            Border tag = e.Source as Border;
            if (e.Source is TextBlock)
                tag = (e.Source as TextBlock).Parent.Parent as Border;
            else if (e.Source is Image)
                tag = (e.Source as Image).Parent.Parent as Border;
            if (tag == null)
                return null;

            return tag.Tag as string;
        }

        private void SelfTags_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            string tag = GetTagTextFromEvent(e);
            if (tag == null)
                return;

            TagsListViewModel context = this.DataContext as TagsListViewModel;
            if (context == null)
                return;

            Prompt prompt = new Prompt();
            Window owner = this.GetOwnerWindow();
            prompt.SetContent("Remove tag", $"Remove the tag {tag}?", "Nevermind", "Remove it");
            prompt.Classes.Add("delete");
            prompt.ShowDialog(owner);
            prompt.Height = 200;
            prompt.Width = 300;
            prompt.CenterOn(owner);

            prompt.OnAccept =()=>{
                context.Tags.Remove(tag);
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
