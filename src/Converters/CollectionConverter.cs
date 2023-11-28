using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    public static class CollectionConverter 
    {
        /// <summary>
        /// Returns false if collection arg is null or empty.
        /// </summary>
        public static readonly IValueConverter HasContent = new FuncValueConverter<object?, bool>((object arg)=> {

            ICollection collection = (ICollection)arg;
            return collection == null || collection.Count == 0 ? false : true;
        });

        /// <summary>
        /// Sorts a string collection asc
        /// </summary>
        public static readonly IValueConverter SortString = new FuncValueConverter<object?, IEnumerable<string>>((object arg) => {

            IEnumerable<string> collection = (IEnumerable<string>)arg;
            if (collection == null)
                return null;

            return collection.OrderBy(t => t);
        });

    }
}
