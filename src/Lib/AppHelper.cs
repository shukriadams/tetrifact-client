namespace TetrifactClient
{
    public class AppHelper
    {
        public static void DoShutdown() 
        {
            Prompt prompt = new Prompt();
            prompt.SetContent("Quit", "Are you sure you want to quit?", "Cancel", "Shutdown");
            prompt.ShowDialog(MainWindow.Instance);
            prompt.CenterOn(MainWindow.Instance);
            prompt.Width = 400;
            prompt.Height = 200;
            prompt.OnAccept = () => {
                MainWindow.Instance.Close();
            };
        }
    }
}
