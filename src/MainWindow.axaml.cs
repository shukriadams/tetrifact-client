using Avalonia.Controls;
using TetrifactClient.Controls;

namespace TetrifactClient;

public partial class MainWindow : Window
{
    public static MainWindow Instance { get; private set; }
    
    private ConsoleWindow _consoleWindow;

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
    }

    private void On_Console_Open(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_consoleWindow != null) 
        {
            _consoleWindow.Close();
            _consoleWindow = null;
        }

        _consoleWindow = new ConsoleWindow();
        _consoleWindow.DataContext = GlobalDataContext.Instance.Console;
        _consoleWindow.Show();
    }
}