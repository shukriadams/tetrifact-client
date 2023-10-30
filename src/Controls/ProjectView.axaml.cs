using Avalonia.Controls;
using System;
using System.ComponentModel;
using System.Linq;

namespace TetrifactClient
{
    public partial class ProjectView : UserControl
    {
        public ProjectView()
        {
            InitializeComponent();
            DataContextChanged += ThisDataContextChanged;

            // if not in v.studio, hide control if it has no data
            //if (!System.Diagnostics.Debugger.IsAttached)
            //    this.IsVisible = false;
        }

        private void ThisDataContextChanged(object? sender, System.EventArgs e)
        {
            this.IsVisible = this.DataContext != null;
            Project thisContext = this.DataContext as Project;
            txtNoBuildsAvailable.IsVisible = !thisContext.Packages.Any();
            gridPackages.IsVisible = thisContext.Packages.Any();
        }

        private void ContextMenu_Opening(object? sender, CancelEventArgs e)
        {

        }

        private void ProjectDelete_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Prompt prompt = new Prompt(300, 400, "Delete", "really?");
            prompt.ShowDialog(MainWindow.Instance);
            prompt.CenterOn(MainWindow.Instance);
            prompt.OnAccept += this.OnDeleteAccept;
        }

        private void OnDeleteAccept() 
        {
            GlobalDataContext.Instance.Projects.Projects.Remove(GlobalDataContext.Instance.FocusedProject);
            GlobalDataContext.Save();
            GlobalDataContext.Instance.FocusedProject = GlobalDataContext.Instance.Projects.Projects.FirstOrDefault();
        }

        private void Button_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            /*
            foreach (var project in GlobalDataContext.Instance.Projects.Projects) 
            {
                foreach (var package in project.Packages) 
                {
                    var list = new List<string>();
                    list = list.Concat(package.Tags).ToList();
                    list.Add("lol");
                    package.Tags = list;
                }
            }

            string test = "";
            */

            Package p = new Package();
            p.Id = Guid.NewGuid().ToString();
            GlobalDataContext.Instance.FocusedProject.Packages.Add(p);
            GlobalDataContext.Instance.FocusedProject.Name = Guid.NewGuid().ToString();
        }
    }
}
