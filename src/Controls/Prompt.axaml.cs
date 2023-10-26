using Avalonia.Controls;
using System.ComponentModel;

namespace TetrifactClient
{
    public partial class Prompt : Window
    {
        public delegate void Cancel();

        public delegate void Accept();

        public Cancel OnCancel;

        public Cancel OnAccept;

        public Prompt(int width = 400, int height = 400, string header = "Hreader", string text = "text")
        {
            InitializeComponent();
            this.Height = height;
            this.Width = width;
            
            txtHeader.Text = header;
            txtText.Text = text;
        }

        public void SetContent(string header, string text, string cancel = "Cancel", string proceed = "Proceed") 
        { 

        }

        private void Cancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
            if (OnCancel != null)
                OnCancel.Invoke();
        }

        private void Proceed_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
            if (OnAccept != null)
                OnAccept.Invoke();
        }

        
    }
}
