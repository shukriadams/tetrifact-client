using Avalonia.Controls;
using System;

namespace TetrifactClient
{
    public partial class PreferencesDialog : Window
    {
        public PreferencesDialog()
        {
            InitializeComponent();
        }

        private void UpdatePackagePath() 
        {
            string newPath = null; // get from form

            try
            {
                // create target directory only as a validation step to ensure that it is valid. The package download daemon will ensure this path independently.
                FileSystemHelper.CreateDirectory(newPath);
            }
            catch (Exception ex)
            {
                Alert alert = new Alert();
                alert.SetContent("Error", ex.Message);
                alert.ShowDialog(this);
                return;
            }
        }

        private void UpdatePackageUnpackPath()
        {
            string newPath = null; // get from form

            try
            {
                // create target directory only as a validation step to ensure that it is valid. The package download daemon will ensure this path independently.
                FileSystemHelper.CreateDirectory(newPath);
            }
            catch (Exception ex)
            {
                Alert alert = new Alert();
                alert.SetContent("Error", ex.Message);
                alert.ShowDialog(this);
                return;
            }
        }
    }
}
