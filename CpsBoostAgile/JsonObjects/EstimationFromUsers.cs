using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CpsBoostAgile.JsonObjects
{
    public class EstimationFromUsers
    {
        public int Estimation { get; set; }
        public int NumberOfVoters { get; set; }
        public List<string> Voters { get; set; }
    }
}