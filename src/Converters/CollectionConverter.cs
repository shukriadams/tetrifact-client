using Avalonia.Data.Converters;
using System.Collections;

namespace TetrifactClient
{
    public static class CollectionConverter 
    {
        public static readonly IValueConverter IsEmpty = new FuncValueConverter<object?, bool>((object arg)=> {

            ICollection collection = (ICollection)arg;
            return collection == null || collection.Count == 0 ? true : false;
        });
    }
}
