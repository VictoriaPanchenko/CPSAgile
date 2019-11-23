using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CpsBoostAgile.Enumeration;

namespace CpsBoostAgile.Models.Retrospective
{
    public class RetrospectiveItemViewModels
    {
        public string Id { get; set; }
        public string RetrospectiveId { get; set; }
        public string CreatedBy { get; set; }
        public RetrospectiveColumnTypeEnum Group { get; set; }
        public string Text { get; set; }
        
        public int Color { get; set; }
        public DateTime? CreatedDate { get; set; }

        public int TotalAssignedVotes { get; set; }
        public int RemainedVotesForParticularUser { get; set; }

        public bool IsEnabledForVoting { get; set; }
    }
}