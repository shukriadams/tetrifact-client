using Avalonia.Threading;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TetrifactClient
{
    /// <summary>
    /// Wraps the process of copying a large number of files. Process is threaded so it doesn't lock up UP, and exposes progress events which can be tracked from UI.
    /// </summary>
    public class BulkCopy
    {
        #region FIELDS

        private TaskCompletionSource<object> _done = new TaskCompletionSource<object>();

        /// <summary>
        /// Set this to true, a child event will get back to you as soon as conveneint to exit the copy process.
        /// </summary>
        private bool _abort;

        /// <summary>
        /// Routes events back to UI.
        /// </summary>
        Dispatcher _dispatcher;

        private string _sourceDirName;

        private string _destDirName;

        private long _filesCount;

        private long _currentFile;

        public event OnProgress OnFileCopied;

        public event SimpleEvent OnError;

        #endregion

        #region PROPERTIES

        public bool OverwriteExisting { get; set; }

        public bool WriteToConsole { get; set; }

        #endregion

        #region CTORS

        public BulkCopy()
        {

        }

        public BulkCopy(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Copied straight from https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="copySubDirs"></param>
        public async Task Copy(string sourceDirName, string destDirName)
        {
            _sourceDirName = sourceDirName;
            _destDirName = destDirName;

            Task.Run(async () => this.Work());
            await _done.Task;
        }

        public void Abort()
        {
            _abort = true;
        }

        private void Work()
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(_sourceDirName);
                if (!dir.Exists)
                    throw new DirectoryNotFoundException($"Source directory {_sourceDirName} does not exist or could not be found.");

                GatherInfo(_sourceDirName);

                CopyDirectories(_sourceDirName, _destDirName);
            }
            finally
            {
                _done.TrySetResult(null);
            }
        }

        private void HandleErrorEvent(object error)
        {
            if (this.WriteToConsole)
                Console.WriteLine(error);

            if (_dispatcher != null)
                _dispatcher.Invoke(() =>
                {
                    OnError?.Invoke(error.ToString());
                });
            else
                OnError?.Invoke(error.ToString());
        }


        private void HandleFileCopyEvent(string file)
        {
            decimal p = (decimal)_currentFile / (decimal)_filesCount;
            int percent = (int)Math.Round((decimal)(p * 100), 0);

            if (this.WriteToConsole)
                Console.WriteLine(file);

            if (_dispatcher != null)
                _dispatcher.Invoke(() =>
                {
                    OnFileCopied?.Invoke(file, _currentFile, _filesCount, percent);
                });
            else
                OnFileCopied?.Invoke(file, _currentFile, _filesCount, percent);
        }

        private void GatherInfo(string sourceDirName)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (_abort)
                return;

            DirectoryInfo[] dirs = dir.GetDirectories();
            FileInfo[] files = dir.GetFiles();

            _filesCount += files.Length;

            // handle folders (recurses)
            foreach (DirectoryInfo subdir in dirs)
                GatherInfo(subdir.FullName);
        }

        private void CopyDirectories(string sourceDirName, string destDirName)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (_abort)
                return;

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            FileSystemHelper.CreateDirectory(destDirName);

            // handle files in this directory
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (_abort)
                    return;

                _currentFile++;

                string targetFile = Path.Combine(destDirName, file.Name);
                bool copy = true;
                if (!this.OverwriteExisting && File.Exists(targetFile))
                {
                    FileInfo targetFileInfo = new FileInfo(targetFile);
                    if (targetFileInfo.LastWriteTime == file.LastWriteTime)
                        copy = false;
                }

                try
                {
                    if (copy)
                        file.CopyTo(targetFile, true);
                    this.HandleFileCopyEvent(targetFile);
                }
                catch (Exception ex)
                {
                    this.HandleErrorEvent(ex);
                }
            }

            // handle directories in this directory, which recurses down
            foreach (DirectoryInfo subdir in dirs)
                CopyDirectories(subdir.FullName, Path.Combine(destDirName, subdir.Name));
        }

        #endregion
    }
}
