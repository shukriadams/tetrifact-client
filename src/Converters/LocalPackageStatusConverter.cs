using Avalonia.Data.Converters;

namespace TetrifactClient
{
    public class LocalPackageStatusConverter
    {
        public static readonly IValueConverter GetStatus = new FuncValueConverter<object?, string>((object arg) => {

            LocalPackage s = (LocalPackage)arg;
            if (s == null)
                return string.Empty;

            return $"{s.TransferState} {s.Status}";
        });
    }
}
