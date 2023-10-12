using System.Collections.Generic;

namespace TetrifactClient.Models
{
    public class GlobalDataContext
    {
        private static GlobalDataContext _instance;

        public IEnumerable<SourceServer> SourceServers { get; } = new List<SourceServer>();





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
