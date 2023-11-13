using System;
using System.Threading.Tasks;

namespace TetrifactClient
{
    /// <summary>
    /// Abstracts away daemon threading. We do this to keep daemon loop + async call out of daemon logic, making daemon logic easier to test.
    /// </summary>
    public class DaemonProcessRunner 
    {
        private bool _busy;

        private ILog _log;

        private int _delayms;

        private AsyncDo _work;

        public bool AllowForceWork { get; set; }
        
        public void Start(AsyncDo work, int delayms, ILog log)
        {
            _log = log;
            _work = work;
            _delayms = delayms;

            Task.Run(async () => {
                while (true)
                {
                    await DoWork();
                } 
            });
        }

        private async Task DoWork() 
        {
            try
            {
                if (_busy)
                    return;

                _busy = true;

                await _work();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Unexpected error running daemon process");
            }
            finally
            {
                _busy = false;

                // force thread pause so this loop doesn't lock CPU
                await Task.Delay(_delayms);
            }
        }

        /// <summary>
        /// Allows for bypassing of timed Work call
        /// </summary>
        public async void WorkNow() 
        {
            if (!AllowForceWork)
                throw new Exception("Force cannot be called on this daemon, please enable explicitly");

            await DoWork();
        }
    }
}
