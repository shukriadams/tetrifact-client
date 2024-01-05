using Avalonia.Controls;
using System;
using System.Linq;

namespace TetrifactClient
{
    public partial class ProjectEditorView : Window
    {
        public ProjectEditorView()
        {
            this.DataContextChanged += This_DataContextChanged;
            InitializeComponent();
        }

        private void This_DataContextChanged(object? sender, EventArgs e)
        {
            ProjectEditorViewModel context = this.DataContext as ProjectEditorViewModel;

            if (context != null && context.Project != null) 
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

            if (project == null)
                lblFormTitle.Content = $"New project";
            else
                lblFormTitle.Content = $"Settings for Project {project.Name}";

            // hide combobox when project already set
            panelFromTemplate.IsVisible = string.IsNullOrEmpty(project.TetrifactServerAddress);
        }

        public new void ShowDialog(Window parent) 
        {
            base.ShowDialog(parent);
            this.CenterOn(parent, true);
        }

        private void OnCancel(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }

        private void ProjectDelete_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ProjectEditorViewModel context = this.DataContext as ProjectEditorViewModel;

            Prompt prompt = new Prompt();
            prompt.SetContent("Delete Project", $"Are you sure you want to permanently delete the project {context.Project.Name}?", "Cancel", "Delete");
            prompt.Height = 300;
            prompt.Width = 400;
            prompt.Classes.Add("delete");
            prompt.ShowDialog(MainWindow.Instance);
            prompt.CenterOn(MainWindow.Instance);
            prompt.OnAccept += this.OnDeleteAccept;
        }
        
        private void OnDeleteAccept()
        {
            ProjectEditorViewModel context = this.DataContext as ProjectEditorViewModel;

            GlobalDataContext.Instance.Projects.Projects.Remove(context.Project);
            GlobalDataContext.Save();
            GlobalDataContext.Instance.FocusedProject = GlobalDataContext.Instance.Projects.Projects.FirstOrDefault();
        }

        private void OnSave(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ProjectEditorViewModel context = this.DataContext as ProjectEditorViewModel;

            if (context.Project.Id == null) 
            {
                GlobalDataContext.Instance.Projects.Projects.Add(new Project
                {
                    Name = txtName.Text,
                    TetrifactServerAddress = txtServer.Text,

                    // id is soft unique int, taken from a fixed date. As we can't generate two projects at same time, this is safe and most importantly, short
                    // give how total path length can be an issue with packages with deep besting, we want to keep every section as short as possible
                    Id = (DateTime.Now.Ticks - new DateTime(2022, 1, 1).Ticks).ToString()
                });
            }
            else 
            {
                context.Project.Name = txtName.Text;
                context.Project.TetrifactServerAddress = txtServer.Text;
                context.Project.IgnoreTags = blockedTagsList.Tags;
                context.Project.RequiredTags= requiredTagsList.Tags;
                context.Project.ApplicationExecutableName = txtExecutableName.Text;
            }

            if (string.IsNullOrEmpty(context.Project.Name))
                return;

            if (string.IsNullOrEmpty(context.Project.TetrifactServerAddress))
                return;

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
