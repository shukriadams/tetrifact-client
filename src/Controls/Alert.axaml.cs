using Avalonia.Controls;

namespace TetrifactClient
{
    public partial class Alert : Window
    {
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
    }
}
