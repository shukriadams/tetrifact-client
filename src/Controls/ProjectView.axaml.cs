using Avalonia.Controls;
using System.ComponentModel;
using System.Linq;

namespace TetrifactClient
{
    public partial class ProjectView : UserControl
    {
        public ProjectView()
        {
            InitializeComponent();
        }



        private void ContextMenu_Opening(object? sender, CancelEventArgs e)
        {
            mnuCopy.IsVisible = false;
        }
    }
}
