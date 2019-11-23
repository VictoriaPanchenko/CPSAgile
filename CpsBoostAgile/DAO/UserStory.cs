using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CpsBoostAgile.DAO
{
    public class UserStory
    {
        public string Id { get; set; }
        public string PokerPlanningId { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public int? FinalEstimation { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? FinishedDate { get; set; }
    }
}