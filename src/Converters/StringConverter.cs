using Avalonia.Data.Converters;

namespace TetrifactClient
{
    public class StringConverter 
    {
        public static readonly IValueConverter IsNullOrEmpty = new FuncValueConverter<object?, bool>((object arg) => {

            string s = (string)arg;
            return string.IsNullOrEmpty(s);
        });

        public static readonly IValueConverter IsNotNullOrEmpty = new FuncValueConverter<object?, bool>((object arg) => {

            string s = (string)arg;
            return !string.IsNullOrEmpty(s);
        });

        
    }
}
