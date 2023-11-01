using Avalonia.Controls;

namespace TetrifactClient
{
    public partial class Alert : Window
    {
        public VoidDo OnAccept;

        public Alert()
        {
            InitializeComponent();
        }

        public void SetContent(string header, string text, string proceed = "Ok")
        {
            txtHeader.Text = header;
            txtText.Text = text;
            btnProceed.Content = proceed;
        }

        private void Proceed_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
            if (OnAccept != null)
                OnAccept.Invoke();
        }
    }
}
