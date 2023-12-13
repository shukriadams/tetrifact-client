using Avalonia.Threading;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace TetrifactClient
{
    public class ConsoleViewModel : ReactiveObject
    {
        private ObservableCollection<string> _items = new ObservableCollection<string> { };

        public int MaxItems = 20;

        public void Add(string item)
        {
            // ensure thread safe or boom
            Dispatcher.UIThread.Post(() => {

                this.Items.Insert(0, item);

            }, DispatcherPriority.Background);

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
