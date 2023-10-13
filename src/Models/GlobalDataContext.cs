using System.Collections.Generic;

namespace TetrifactClient
{
    public class GlobalDataContext
    {
        private static GlobalDataContext _instance;

        public IEnumerable<SourceServer> SourceServers { get; } = new List<SourceServer>();

        public ProjectsViewModel Projects { get; } = new ProjectsViewModel();

        private string caption = "some text";

        public string Caption
        {
            get => caption;
            set => caption = value;
        }

        public static GlobalDataContext Instance 
        { 
            get 
            {
                if (_instance == null)
                {
                    _instance = new GlobalDataContext();

                }

                return _instance;
            } 
        }
    }
}
