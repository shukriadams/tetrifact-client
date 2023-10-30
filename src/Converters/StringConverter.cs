using Avalonia.Data.Converters;
using System;

namespace TetrifactClient
{
    public class StringConverter 
    {
        public static readonly IValueConverter IsNullOrEmpty = new FuncValueConverter<object?, bool>((object arg) => {

            string s = (string)arg;
            return !string.IsNullOrEmpty(s);
        });
    }
}
