using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CpsBoostAgile.Enumeration;

namespace CpsBoostAgile.DAO
{
    public class RetrospectiveItem
    {
        public string Id { get; set; }
        public string RetrospectiveId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public RetrospectiveColumnTypeEnum Group { get; set; }
        public string Text { get; set; }
        public int Rating { get; set; }
    }
}