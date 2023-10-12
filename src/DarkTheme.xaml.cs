using Avalonia.Markup.Xaml;
using AvaloniaStyles = Avalonia.Styling.Styles;

namespace TetrifactClient;

public class DarkTheme : AvaloniaStyles
{
    public DarkTheme() => AvaloniaXamlLoader.Load(this);
}