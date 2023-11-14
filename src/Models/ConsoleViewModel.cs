using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace TetrifactClient
{
    public class ConsoleViewModel : ReactiveObject
    {
        private ObservableCollection<string> _items = new ObservableCollection<string> { };

        public int MaxItems = 2;

        public void Add(string item)
        {
            this.Items.Insert(0, item);
            if (this.Items.Count > MaxItems)
                this.Items = new ObservableCollection<string>(Items.Take(MaxItems));
        }

        public ObservableCollection<string> Items
        {
            get => _items;
            private set => this.RaiseAndSetIfChanged(ref _items, value);
        }
    }
}
