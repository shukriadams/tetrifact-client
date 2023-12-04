using Avalonia.Data.Converters;

namespace TetrifactClient
{
    public class ProjectStatusConverter
    {
        public static readonly IValueConverter GetLatest = new FuncValueConverter<object?, string>((object arg) => {

            Project project= arg as Project;
            if (arg == null)
                return string.Empty;

            return project.CurrentStatus;
        });
    }
}
