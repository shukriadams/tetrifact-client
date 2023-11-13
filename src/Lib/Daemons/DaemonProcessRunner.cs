using System;
using System.Threading.Tasks;

namespace TetrifactClient
{
    /// <summary>
    /// Abstracts away daemon threading. We do this to keep daemon loop + async call out of daemon logic, making daemon logic easier to test.
    /// </summary>
    public class DaemonProcessRunner 
    {
        public void Start(AsyncDo work, int delayms, ILog log)
        {
            bool busy = false;

            Task.Run(async () => {
                while (true)
                {
                    try
                    {
                        if (busy)
                            continue;

                        busy = true;

                        await work();
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, $"Unexpected error running daemon process");
                    }
                    finally
                    {
                        busy = false;

                        // force thread pause so this loop doesn't lock CPU
                        await Task.Delay(delayms);
                    }
                } 
            });
        }
    }
}
