using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CpsBoostAgile.DAO
{
    public class Retrospective 
    {
        public string Id { get; set; }
        public bool EnableVoting { get; set; }
        public int VotesPerUser { get; set; }
        public virtual ICollection<RetrospectiveItem> RetrospectiveItems { get; set; }
    }
}