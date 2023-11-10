using System.Collections.Generic;

namespace TetrifactClient
{
    public class GlobalDataContextSerialize
    {
        public IEnumerable<Project> Projects { get; set; }

        public string DataFolder { get; set; }
    }
}
