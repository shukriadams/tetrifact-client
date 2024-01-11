using Avalonia.Controls;
using TetrifactClient.Controls;

namespace TetrifactClient;

public partial class MainWindow : Window
{
    public static MainWindow Instance { get; private set; }
    
    private Console _consoleWindow;

    private Settings _settingsWindow;

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        //this.win StateChanged += MainWindow_StateChanged;
    }

    private void On_Console_Open(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_consoleWindow != null) 
        {
            _consoleWindow.Close();
            _consoleWindow = null;
        }

        _consoleWindow = new Console();
        _consoleWindow.DataContext = GlobalDataContext.Instance.Console;
        _consoleWindow.Show();
    }

    private void On_Settings_Open(object? sender, Avalonia.Interactivity.RoutedEventArgs e) 
    {
        if (_settingsWindow != null)
        {
            _settingsWindow.Close();
            _settingsWindow = null;
        }

        _settingsWindow = new Settings();
        _settingsWindow.DataContext = GlobalDataContext.Instance.Console;
        _settingsWindow.ShowDialog(MainWindow.Instance);
    }

    private void On_App_Close(object? sender, Avalonia.Interactivity.RoutedEventArgs e) 
    {
        AppHelper.DoShutdown();
    }
}