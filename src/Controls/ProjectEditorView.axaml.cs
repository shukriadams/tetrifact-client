using Avalonia.Controls;
using System;

namespace TetrifactClient
{
    public partial class ProjectEditorView : Window
    {
        public ProjectEditorView()
        {
            this.DataContextChanged += ProjectView_DataContextChanged;
            InitializeComponent();
        }

        private void ProjectView_DataContextChanged(object? sender, EventArgs e)
        {
            ProjectEditorViewModel context = this.DataContext as ProjectEditorViewModel;
            if (context != null) 
            {
                requiredTagsList.SetContext(context.Project.CommonTags, context.Project.RequiredTags);
                blockedTagsList.SetContext(context.Project.CommonTags, context.Project.IgnoreTags);
            }
        }

        public void SetContext(Project project) 
        {
            this.DataContext = new ProjectEditorViewModel 
            {
                ProjectTemplates = GlobalDataContext.Instance.ProjectTemplates,
                Project = project
            };

            // hide combobox when project already set
            cmbTemplateSource.IsVisible = project == null;
        }

        public new void ShowDialog(Window parent) 
        {
            base.ShowDialog(parent);
            this.CenterOn(parent);
        }

        private void OnCancel(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnSave(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ProjectEditorViewModel context = this.DataContext as ProjectEditorViewModel;
            if (context.Project == null) 
            {
                GlobalDataContext.Instance.Projects.Projects.Add(new Project
                {
                    Name = txtName.Text,
                    TetrifactServerAddress = txtServer.Text
                });
            }
            else 
            {
                context.Project.Name = txtName.Text;
                context.Project.TetrifactServerAddress = txtServer.Text;
                context.Project.IgnoreTags = blockedTagsList.Tags;
                context.Project.RequiredTags= requiredTagsList.Tags;
            }

            GlobalDataContext.Save();
            this.Close();
        }

        private void OnTemplateSourceChanged(object? sender, SelectionChangedEventArgs e)
        {
            Project projectTemplate = (Project)cmbTemplateSource.SelectedValue;
            if (projectTemplate == null)
                return;

            txtName.Text = projectTemplate.Name;
            txtServer.Text = projectTemplate.TetrifactServerAddress;
        }
    }
}
