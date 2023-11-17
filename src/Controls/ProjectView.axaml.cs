using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Security;

namespace TetrifactClient
{
    public partial class ProjectView : UserControl
    {
        public ProjectView()
        {
            InitializeComponent();

            // if not in v.studio, hide control if it has no data
            //if (!System.Diagnostics.Debugger.IsAttached)
            //    this.IsVisible = false;

            gridPackages.DataContextChanged += GridDataChanged;

        }

        private void GridDataChanged(object? sender, System.EventArgs e)
        {
            Project datacontext = gridPackages.DataContext as Project;
            if (datacontext != null) 
                datacontext.Packages.CollectionChanged += gridChanged;

            this.SetVisualState();
        }

        private void SetVisualState() 
        {
            Project datacontext = gridPackages.DataContext as Project;
            gridPackages.IsVisible = datacontext.Packages.Any();
            txtNoBuildsAvailable.IsVisible = !datacontext.Packages.Any();

            this.IsVisible = this.DataContext != null;
        }

        private void gridChanged(object? sender, System.EventArgs e) 
        {
            this.SetVisualState();
        }

        private void ContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            LocalPackage selectedProject = gridPackages.SelectedItem as LocalPackage;
            if (selectedProject == null)
                return;

            mnuDownload.IsVisible = selectedProject.IsDownloadable();
            mnuDelete.IsVisible = selectedProject.IsExecutable();
            mnuIgnore.IsVisible = selectedProject.IsDownloadable() && !selectedProject.Ignore;
            mnuUnignore.IsVisible = selectedProject.IsDownloadable() && !mnuIgnore.IsVisible;
            mnuRun.IsVisible = selectedProject.IsExecutable();
            mnuVerify.IsVisible = selectedProject.IsExecutable();
            mnuKeep.IsVisible = selectedProject.IsExecutable() && !selectedProject.Keep;
            mnuUnkeep.IsVisible = selectedProject.IsExecutable() && !mnuKeep.IsVisible;
            mnuViewInExplorer.IsVisible = selectedProject.IsExecutable();
            mnuVerify.IsVisible = selectedProject.IsExecutable();
        }

        private void OnIgnoreClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) 
        {
            LocalPackage selectedProject = gridPackages.SelectedItem as LocalPackage;
            if (selectedProject == null)
                return;

            selectedProject.Ignore = true;
        }

        private void OnUnignoreClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LocalPackage selectedProject = gridPackages.SelectedItem as LocalPackage;
            if (selectedProject == null)
                return;

            selectedProject.Ignore = false;
        }

        private void OnTagsClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LocalPackage selectedProject = gridPackages.SelectedItem as LocalPackage;
            if (selectedProject == null)
                return;
            
            TagsDialog dialog = new TagsDialog();
            dialog.ShowDialog(MainWindow.Instance);
        }

        private void OnKeepClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LocalPackage selectedProject = gridPackages.SelectedItem as LocalPackage;
            if (selectedProject == null)
                return;

            selectedProject.Keep = true;
        }

        private void OnUnkeepClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LocalPackage selectedProject = gridPackages.SelectedItem as LocalPackage;
            if (selectedProject == null)
                return;

            selectedProject.Keep = false;
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

            selectedProject.TransferState = PackageTransferStates.UserMarkedForDownload;
            PackageDownloadDaemon daemon = App.Daemons.First(d => d.GetType() == typeof(PackageDownloadDaemon)) as PackageDownloadDaemon;
            daemon.WorkNow();
        }

        private void OnDeletePackage(object? sender, Avalonia.Interactivity.RoutedEventArgs e) 
        {
            LocalPackage selectedProject = gridPackages.SelectedItem as LocalPackage;
            if (selectedProject == null)
                return;

            selectedProject.TransferState = PackageTransferStates.UserMarkedForDelete;
        }

        private void OnDeleteAccept() 
        {
            GlobalDataContext.Instance.Projects.Projects.Remove(GlobalDataContext.Instance.FocusedProject);
            GlobalDataContext.Save();
            GlobalDataContext.Instance.FocusedProject = GlobalDataContext.Instance.Projects.Projects.FirstOrDefault();
        }


    }
}
