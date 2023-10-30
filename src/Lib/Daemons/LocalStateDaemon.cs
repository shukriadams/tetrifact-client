using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TetrifactClient.Models;

namespace TetrifactClient.Lib.Daemons
{
    /// <summary>
    /// 
    /// </summary>
    public class LocalStateDaemon
    {
        private bool _busy;
        private int _delay = 5000;

        public void Start()
        {
            Task.Run(async () => {
                while (true)
                {
                    try
                    {
                        if (_busy)
                            continue;

                        _busy = true;

                        await this.Work();
                    }
                    catch (Exception ex)
                    {
                        GlobalDataContext.Instance.Console.Add($"Unexpected error getting package manifest : {ex.Message}");
                        // todo : write ex to log file
                    }
                    finally
                    {
                        _busy = false;

                        // force thread pause so this loop doesn't lock CPU
                        await Task.Delay(_delay);
                    }
                } // while
            });
        }

        private async Task Work()
        {
            foreach (Project project in GlobalDataContext.Instance.Projects.Projects) 
            {

            }

        }
    }
}
