using Avalonia.Controls;

namespace TetrifactClient
{
    public partial class Prompt : Window
    {

        public VoidDo OnCancel;

        public VoidDo OnAccept;

        public Prompt()
        {
            InitializeComponent();
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
