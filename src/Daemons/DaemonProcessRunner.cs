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

        private bool _pause;

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
            if (_busy)
                return;

            _busy = true;
            _pause = true;

            try
            {
                await _work();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Unexpected error running daemon process");
            }
            finally
            {

                // force thread pause so this loop doesn't lock CPU
                DateTime start = DateTime.Now;
                while (_pause && (DateTime.Now - start).TotalMilliseconds < _delayms) 
                    await Task.Delay(100); // delay .1 seconds only, we want fast response

                _pause = false;
                _busy = false;
            }
        }

        /// <summary>
        /// Allows for bypassing of timed Work call
        /// </summary>
        public async void WorkNow() 
        {
            _pause = false;
            await DoWork();
        }
    }
}
