using Avalonia.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unity;

namespace TetrifactClient
{
    public partial class ProjectView : UserControl
    {
        ILog _log;
        
        bool _gridChangeBound = false;

        public ProjectView()
        {
            InitializeComponent();
            txtNoBuildsAvailable.Text = "Checking content ... ";
            gridPackages.DataContextChanged += GridDataChanged;

            _log = new Log();
        }

        private void GridDataChanged(object? sender, System.EventArgs e)
        {
            Project contextProject = gridPackages.DataContext as Project;

            if (!_gridChangeBound && contextProject != null)
            {
                contextProject.Packages.CollectionChanged += gridChanged;
                _gridChangeBound = true;
            }

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

        private void OnRunPackage(object? sender, Avalonia.Interactivity.RoutedEventArgs e) 
        {
            LocalPackage selectedProject = gridPackages.SelectedItem as LocalPackage;
            if (selectedProject == null)
                return;

            Project project = gridPackages.DataContext as Project;
            PackageRunner runner = new PackageRunner();
            runner.Run(GlobalDataContext.Instance, selectedProject, project);
        }

        private void OnViewInExplorer(object? sender, Avalonia.Interactivity.RoutedEventArgs e) 
        {
            LocalPackage selectedProject = gridPackages.SelectedItem as LocalPackage;
            if (selectedProject == null)
                return;

            Project project = gridPackages.DataContext as Project;

            string packageDirectory = PathHelper.GetPackageContentDirectoryPath(GlobalDataContext.Instance, project , selectedProject);
            if (Directory.Exists(packageDirectory))
            {
                try
                {
                    // note : this is windows specific, need to find a cross-os friendly version 
                    Process.Start("explorer.exe", packageDirectory);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, $"Error opening path {packageDirectory}");
                }
            }
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

        private void ProjectEdit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) 
        {
            ProjectEditorView editor = App.UnityContainer.Resolve<ProjectEditorView>();
            editor.SetContext(gridPackages.DataContext as Project);
            editor.ShowDialog(MainWindow.Instance);
        }

        private void ProjectDelete_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Project context = this.DataContext as Project;

            Prompt prompt = new Prompt();
            prompt.SetContent("Delete Project", $"Are you sure you want to permanently delete the project {context.Name}?");
            prompt.Height = 300;
            prompt.Width = 400;
            prompt.Classes.Add("delete");
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
