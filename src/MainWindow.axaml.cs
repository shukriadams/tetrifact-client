using Avalonia.Controls;
using TetrifactClient.Controls;

namespace TetrifactClient;

public partial class MainWindow : Window
{
    public static MainWindow Instance { get; private set; }
    
    private ConsoleWindow _consoleWindow;

    private SettingsForm _settingsWindow;

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

    private void On_Settings_Open(object? sender, Avalonia.Interactivity.RoutedEventArgs e) 
    {
        if (_settingsWindow != null)
        {
            _settingsWindow.Close();
            _settingsWindow = null;
        }

        _settingsWindow = new SettingsForm();
        _settingsWindow.DataContext = GlobalDataContext.Instance.Console;
        _settingsWindow.CenterOn(MainWindow.Instance);
        _settingsWindow.ShowDialog(MainWindow.Instance);
    }
}