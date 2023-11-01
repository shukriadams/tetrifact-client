using Avalonia.Controls;

namespace TetrifactClient
{
    public partial class Prompt : Window
    {

        public VoidDo OnCancel;

        public VoidDo OnAccept;

        public Prompt(int width = 400, int height = 400, string header = "Header", string text = "text")
        {
            InitializeComponent();
            this.Height = height;
            this.Width = width;

            this.SetContent(header, text);
        }

        public void SetContent(string header, string text, string cancel = "Cancel", string proceed = "Proceed") 
        {
            txtHeader.Text = header;
            txtText.Text = text;
            btnCancel.Content = cancel;
            btnProceed.Content = proceed;
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
