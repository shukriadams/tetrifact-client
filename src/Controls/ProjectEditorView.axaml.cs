using Avalonia.Controls;
using System;

namespace TetrifactClient
{
    public partial class ProjectEditorView : Window
    {
        public ProjectEditorView()
        {
            InitializeComponent();
        }

        public new void ShowDialog(Window parent) 
        {
            base.ShowDialog(parent);
            this.CenterOn(parent);
        }
    }
}
