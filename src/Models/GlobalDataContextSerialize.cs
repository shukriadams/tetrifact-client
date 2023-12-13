using System.Collections.Generic;

namespace TetrifactClient
{
    public class GlobalDataContextSerialize
    {
        public IEnumerable<Project> Projects { get; set; }

        public string ProjectsRootDirectory { get; set; }

        public int Timeout { get; set; }
    }
}
