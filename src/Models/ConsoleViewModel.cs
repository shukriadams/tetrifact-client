using ReactiveUI;
using System.Collections.ObjectModel;

namespace TetrifactClient
{
    public class ConsoleViewModel : ReactiveObject
    {
        private ObservableCollection<string> _items = new ObservableCollection<string> { };

        public ObservableCollection<string> Items
        {
            get => _items;
            set => this.RaiseAndSetIfChanged(ref _items, value);
        }
    }
}
