using Avalonia.Data.Converters;
using Avalonia.Media;

namespace TetrifactClient
{
    public static class ConnectionQualityConverter
    {
        public static readonly IValueConverter ToNamedColor = new FuncValueConverter<object?, IBrush>((object arg) => {

            ConnectionQuality quality = (ConnectionQuality)arg;
            return quality == ConnectionQuality.Broken ? Brush.Parse("Red") :
                quality == ConnectionQuality.Good ? Brush.Parse("Green") :
                quality == ConnectionQuality.Degraded ? Brush.Parse("Orange") :
                Brush.Parse("White");
        });

        public static readonly IValueConverter ToTooltipString = new FuncValueConverter<object?, string>((object arg) =>
        {
            ConnectionQuality quality = (ConnectionQuality)arg;
            return quality == ConnectionQuality.Broken ? "Server unreachable" :
                quality == ConnectionQuality.Good  ? "Server connection fast" :
                quality == ConnectionQuality.Degraded ? "Server connection poor" :
                "Server connection calculating, please wait...";
        });
    }
}
