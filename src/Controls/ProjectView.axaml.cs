using Avalonia.Controls;
using System.ComponentModel;

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
        }

        private void ContextMenu_Opening(object? sender, CancelEventArgs e)
        {
            mnuCopy.IsVisible = false;
        }

        private void ProjectDelete_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Prompt prompt = new Prompt(300, 400, "Delete", "really?");
            prompt.ShowDialog(MainWindow.Instance);
            prompt.CenterOn(MainWindow.Instance);
        }
    }
}
