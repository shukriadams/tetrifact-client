using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Platform;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TetrifactClient.Controls;
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
            gridPackages.DataContextChanged += GridDataChanged;
            txtNoBuildsAvailable.Text = string.Empty;
            _log = new Log();
            SetVisualState();
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
            if (datacontext != null)
            {
                gridPackages.IsVisible = datacontext.Packages.Any();
                txtNoBuildsAvailable.IsVisible = !datacontext.Packages.Any();
            }

            // we always want to both project + no-project content in design mode
            if (!Design.IsDesignMode)
            {
                bool show = this.DataContext != null;
                projectContent.IsVisible = show;
                noProjectContent.IsVisible = !show;
            }
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
            mnuDelete.IsVisible = selectedProject.IsDeletable();
            mnuIgnore.IsVisible = selectedProject.IsDownloadable() && !selectedProject.Ignore;
            mnuUnignore.IsVisible = selectedProject.IsDownloadable() && !mnuIgnore.IsVisible;
            mnuCanceldownload.IsVisible = selectedProject.IsQueuedForDownloadorDownloading();
            mnuRun.IsVisible = selectedProject.IsExecutable();
            mnuVerify.IsVisible = selectedProject.IsVerifiable();
            mnuKeep.IsVisible = selectedProject.IsExecutable() && !selectedProject.Keep;
            mnuUnkeep.IsVisible = selectedProject.IsExecutable() && !mnuKeep.IsVisible;
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

        private void OnVerifyClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) 
        {
            LocalPackage selectPackage = gridPackages.SelectedItem as LocalPackage;
            if (selectPackage == null)
                return;

            selectPackage.DownloadProgress.Message = "Verifying ...";
            selectPackage.IsQueuedForVerify = true;
        }

        private void OnTagsClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LocalPackage selectedPackage = gridPackages.SelectedItem as LocalPackage;
            if (selectedPackage == null)
                return;
        }

        private void OnKeepClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LocalPackage selectedPackage = gridPackages.SelectedItem as LocalPackage;
            if (selectedPackage == null)
                return;

            selectedPackage.Keep = true;
        }

        private async void OnCopyPackageId(object? sender, Avalonia.Interactivity.RoutedEventArgs e) 
        {
            LocalPackage selectedPackage = gridPackages.SelectedItem as LocalPackage;
            if (selectedPackage == null)
                return;

            IClipboard clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            DataObject dataObject = new DataObject();
            dataObject.Set(DataFormats.Text, selectedPackage.Package.Id);
            await clipboard.SetDataObjectAsync(dataObject);
        }

        private void OnUnkeepClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LocalPackage selectedPackage = gridPackages.SelectedItem as LocalPackage;
            if (selectedPackage == null)
                return;

            selectedPackage.Keep = false;
        }

        private void ProjectEdit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) 
        {
            ProjectEdit editor = App.UnityContainer.Resolve<ProjectEdit>();
            editor.SetContext(gridPackages.DataContext as Project);
            editor.ShowDialog(MainWindow.Instance);
        }

        private void OnCancelDownloadClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) 
        {
            LocalPackage selectedProject = gridPackages.SelectedItem as LocalPackage;
            if (selectedProject == null)
                return;

            selectedProject.CancelQueueState();
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

        private void OnLoadRow(object sender, DataGridRowEventArgs e) 
        {
            DataGridRow row = e.Row;
            LocalPackage package = e.Row.DataContext as LocalPackage;
            row.Bind(DataGridRow. BackgroundProperty, 
                new Binding("TransferState", BindingMode.OneWay) { Converter = new RowSatusConverter() }, 
                package);
        }

        private void OnFocusPackage() 
        {
            lblFocusPackageFeedback.Text = string.Empty;

            string focusPackageId = txtFocusPackage.Text;
            if (string.IsNullOrEmpty(focusPackageId))
            {
                lblFocusPackageFeedback.Text = "Required";
                return;
            }

            focusPackageId = focusPackageId.Trim();

            Project project = gridPackages.DataContext as Project;
            if (project == null)
            {
                lblFocusPackageFeedback.Text = "No matches";
                return;
            }

            LocalPackage focusedPackage = project.Packages.FirstOrDefault(p => p.Package.Id == focusPackageId);
            if (focusedPackage == null)
                focusedPackage = project.Packages.FirstOrDefault(p => p.Package.Id.ToLower().Contains(focusPackageId.ToLower()));

            // no feedback, this is unlikely
            if (focusedPackage == null)
            {
                lblFocusPackageFeedback.Text = "No matches";
                return;
            }

            gridPackages.SelectedItem = focusedPackage;
            gridPackages.ScrollIntoView(gridPackages.SelectedItem, null);
        }

        private void OnFocusPackage(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.OnFocusPackage();
        }

        private void FocusPackage_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter)
                this.OnFocusPackage();
        }
    }
}
