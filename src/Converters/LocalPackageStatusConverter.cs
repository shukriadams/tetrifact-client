using Avalonia.Data.Converters;

namespace TetrifactClient
{
    public class LocalPackageStatusConverter
    {
        public static readonly IValueConverter GetState = new FuncValueConverter<object?, string>((object arg) => {

            BuildTransferStates? s = (BuildTransferStates)arg;
            if (s == null)
                return string.Empty;

            return $"{s}";
        });
    }
}
