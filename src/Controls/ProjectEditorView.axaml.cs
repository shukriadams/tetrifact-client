using Avalonia.Controls;

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

        private void OnCancel(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnSave(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {

            GlobalDataContext.Instance.Projects.Projects.Add(new Project
            {
                Name = txtName.Text,
                TetrifactServerAddress = txtServer.Text
            });

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
