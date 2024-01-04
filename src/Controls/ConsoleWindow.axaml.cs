using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Input;

namespace TetrifactClient.Controls
{
    public partial class ConsoleWindow : Window
    {
        public ConsoleWindow()
        {
            InitializeComponent();
        }

        private void ContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private async void OnCopyConsoleItem(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string selectedLine = listProjects.SelectedItem as string;
            if (selectedLine == null)
                return;

            IClipboard clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            DataObject dataObject = new DataObject();
            dataObject.Set(DataFormats.Text, selectedLine);
            await clipboard.SetDataObjectAsync(dataObject);
        }
    }
}
