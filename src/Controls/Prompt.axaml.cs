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

            TopLevel topLevel = TopLevel.GetTopLevel(this)!;
            topLevel.KeyDown += TopLevel_KeyDown;
        }

        private void TopLevel_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Escape)
                this.Cancel();
            else if (e.Key == Avalonia.Input.Key.Enter)
                this.Accept();

        }

        private void Accept() 
        {
            this.Close();
            if (OnAccept != null)
                OnAccept.Invoke();
        }

        private void Cancel() 
        {
            this.Close();
            if (OnCancel != null)
                OnCancel.Invoke();

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
            this.Cancel();
        }

        private void Proceed_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Accept();
        }
    }
}
