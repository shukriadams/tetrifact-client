using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TetrifactClient
{
    public class AppStart
    {
        // Logic to execute on app start.
        public void Work() 
        {
            ILog log = new Log();

            // clean out any hanging unpacks
            foreach (Project project in GlobalDataContext.Instance.Projects.Projects) 
            {
                string projectDirectory = Path.Join(GlobalDataContext.Instance.ProjectsRootDirectory, project.Id);
                string[] packagedirectories = new string[0];

                // no need to clean projects that haven't been created on disk yet
                if (!Directory.Exists(projectDirectory))
                    continue;

                try
                {
                    packagedirectories = Directory.GetDirectories(projectDirectory);
                }
                catch (Exception ex)
                {
                    continue;
                }

                foreach (string dir in packagedirectories) 
                {
                    IEnumerable<string> hangingUnpacks = Directory.GetDirectories(dir)
                        .Where(dir => Path.GetFileName(dir).StartsWith("~"));

                    // auto mark any existing unpack directories for delete
                    foreach (string hangingUnpack in hangingUnpacks) 
                    {
                        string movePath = Path.Combine(Path.GetDirectoryName(hangingUnpack), $"!{Path.GetFileName(hangingUnpack)}");
                        
                        try
                        {
                            Directory.Move(hangingUnpack, movePath);
                        }
                        catch (Exception ex)
                        {
                            // directories are not supposed to be locked at app start, log 
                            log.LogError(ex);
                        }
                    }
                }
            }

        }
    }
}
