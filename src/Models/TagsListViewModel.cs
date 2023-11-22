using System.Collections.Generic;

namespace TetrifactClient
{
    public class TagsListViewModel
    {
        public IEnumerable<string> ExistingTags { get; set; }

        public IList<string> Tags { get; set; }

        public TagsListViewModel() 
        {
            this.ExistingTags = new string[0];
            this.Tags = new List<string>(); 
        }
    }
}
