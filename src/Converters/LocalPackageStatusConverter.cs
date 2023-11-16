using Avalonia.Data.Converters;

namespace TetrifactClient
{
    public class LocalPackageStatusConverter
    {
        public static readonly IValueConverter GetState = new FuncValueConverter<object?, string>((object arg) => {

            PackageTransferStates? s = (PackageTransferStates)arg;
            if (s == null)
                return string.Empty;

            if (s == PackageTransferStates.UserMarkedForDownload)
                return "Downloading";
            
            if (s == PackageTransferStates.AutoMarkedForDownload)
                return "Downloading";

            if (s == PackageTransferStates.AvailableForDownload)
                return "Available";

            return $"{s}";
        });
    }
}
