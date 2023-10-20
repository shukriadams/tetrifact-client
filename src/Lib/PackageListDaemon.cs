using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace TetrifactClient
{
    /// <summary>
    /// Updates package list for all projects
    /// </summary>
    public class PackageListDaemon
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

                        // do work here
                        this.Work();
                    }
                    catch (Exception ex)
                    {
                        GlobalDataContext.Instance.Console.Items.Add($"Unexpected error getting package manifest from build server {ex.Message}");
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

        private void Work()
        {
            HardenedWebClient client = new HardenedWebClient();

            foreach (Project project in GlobalDataContext.Instance.Projects.Projects) 
            {
                
                /*
                
                download attempt
                - payload (binary or stream)
                - time taken
                - contact attempts
                - did it succeed
                - error description if applicable

                 */

            }
        }
    }
}
