using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public class GlobalDataContextSerialize
    {
        public IEnumerable<Project> Projects { get; set; }

        public string DataFolder { get; set; }
    }
}
