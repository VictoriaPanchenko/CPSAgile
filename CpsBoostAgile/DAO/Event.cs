using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CpsBoostAgile.Enumeration;

namespace CpsBoostAgile.DAO
{
    public class Event
    {
        public string Id { get; set; }
        public EventTypeEnum EventType { get; set; }
        public string Name { get; set; }
        public string ProjectName { get; set; }
        public string TeamName { get; set; }
        public string Comment { get; set; }
        public string Sprint { get; set; }
        public PhaseEnum Phase { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? FinishedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}