using Avalonia.Controls;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        private void OnDownloadClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LocalPackage selectedProject = gridPackages.SelectedItem as LocalPackage;
            if (selectedProject == null)
                return;

            selectedProject.TransferState = BuildTransferStates.UserMarkedForDownload;
        }

        private void OnDeleteAccept() 
        {
            GlobalDataContext.Instance.Projects.Projects.Remove(GlobalDataContext.Instance.FocusedProject);
            GlobalDataContext.Save();
            GlobalDataContext.Instance.FocusedProject = GlobalDataContext.Instance.Projects.Projects.FirstOrDefault();
        }
    }
}
