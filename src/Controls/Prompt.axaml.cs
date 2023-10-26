using Avalonia.Controls;

namespace TetrifactClient.Controls
{
    public partial class Prompt : Window
    {
        public Prompt()
        {
            InitializeComponent();
        }

        public void SetContent(string header, string text, string cancel = "Cancel", string proceed = "Proceed") 
        { 

        }

        private void Cancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
        }

        private void Proceed_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
        }

        
    }
}
