using Avalonia.Data.Converters;
using System;
using System.Collections;

namespace TetrifactClient
{
    public class ListContentToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ICollection collection = (ICollection)value;
            return collection == null || collection.Count == 0 ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
