using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace TetrifactClient
{
    public class RowSatusConverter : IValueConverter
    {
        public static readonly RowSatusConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            IBrush defaultColor = Brush.Parse("#4F4557");
            if (value == null)
                return defaultColor;

            PackageTransferStates transFerState = (PackageTransferStates)Enum.Parse(typeof(PackageTransferStates), value.ToString());

            if (transFerState == PackageTransferStates.Corrupt || transFerState == PackageTransferStates.DownloadFailed)
                return Brush.Parse("#781c1c");

            if (transFerState == PackageTransferStates.Downloaded)
                return Brush.Parse("#426e57");

            if (transFerState == PackageTransferStates.Deleting || transFerState == PackageTransferStates.AutoMarkedForDelete || transFerState == PackageTransferStates.UserMarkedForDelete)
                return Brush.Parse("#763794");

            return defaultColor;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
