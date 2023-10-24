using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.IO;
using Unity;


namespace TetrifactClient;

public partial class App : Application
{
    public static IUnityContainer UnityContainer { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Styles.Add(new DarkTheme());
    }

    /// <summary>
    /// Cludge solution to allow any class to reliably ensure type binding. This is needed by UI controls in Visual studio only, unity
    /// binding doesn't fire reliably in visual studio, so designer views often crash when trying to display a control that has a child
    /// control that uses unity.
    /// </summary>
    public static void EnsureUnity()
    {
        if (UnityContainer != null)
            return;

        UnityContainer = new UnityContainer();
        UnityContainer.RegisterType<ProjectEditorView, ProjectEditorView>();
        UnityContainer.RegisterType<ILog, Log>();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        EnsureUnity();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = GlobalDataContext.Instance,
            };
        }

        base.OnFrameworkInitializationCompleted();

        // create directories
        Directory.CreateDirectory(GlobalDataContext.Instance.GetProjectsDirectoryPath());

        // start daemons
        PackageListDaemon daemon = new PackageListDaemon();
        PackageDetailsDaemon daemon2 = new PackageDetailsDaemon();
        daemon.Start();
        daemon2.Start();


    }
}