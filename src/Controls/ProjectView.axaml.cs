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
        }

        private void ThisDataContextChanged(object? sender, System.EventArgs e)
        {

        }

        private void ContextMenu_Opening(object? sender, CancelEventArgs e)
        {
            mnuCopy.IsVisible = false;
        }
    }
}
