using Avalonia.Data.Converters;
using Avalonia.Media;
using System;

namespace TetrifactClient
{
    public static class ConnectionQualityConverter
    {
        public static readonly IValueConverter ToNamedColor = new FuncValueConverter<object?, IBrush>((object arg) => {

            ConnectionQuality quality = (ConnectionQuality)arg;
            return quality == ConnectionQuality.Broken ? Brush.Parse("Red") :
                quality == ConnectionQuality.Good ? Brush.Parse("Green") :
                quality == ConnectionQuality.Degraded ? Brush.Parse("Orange") :
                Brush.Parse("Purple");
        });

        public static readonly IValueConverter ToDisplayString = new FuncValueConverter<object?, string>((object arg) => {
            double quality = (double)arg;
            return $"{Math.Round(quality, 1)}ms";
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
