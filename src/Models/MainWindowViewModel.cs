using Avalonia.Media.Imaging;
using ReactiveUI;

namespace TetrifactClient
{
    public class MainWindowViewModel : ReactiveObject
    {
        public string ApplicationName { get; private set; }

        /// <summary>
        /// in seconds
        /// </summary>
        public int TetrifactPollInterval { get; private set; }

        public int MaxDownloadFailedAttempts { get; set; }

        public int MaxThreadsPerDownload { get; set; }

        public Bitmap? ImageFromBinding { get; }

        private string caption = "some text";
   
        public string Caption
        {
            get => caption;
            set => this.RaiseAndSetIfChanged(ref caption, value);
        }




        public MainWindowViewModel() 
        {
            // load stuff from file or resource here
            this.ApplicationName = "load-appname-here";
        }
    }
}
