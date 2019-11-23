using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using CpsBoostAgile.DAO;
using CpsBoostAgile.Enumeration;

namespace CpsBoostAgile.Models.PokerPlanning
{
    public class PPViewModels
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Name*")]
        [StringLength(maximumLength: 30)]
        public string EventName { get; set; }

        [Display(Name = "Project")]
        [StringLength(maximumLength: 30)]
        public string Project { get; set; }

        [Required]
        [Display(Name = "Sprint*")]
        [StringLength(maximumLength: 30)]
        public string Sprint { get; set; }

        [Required]
        [Display(Name = "Team*")]
        [StringLength(maximumLength: 30)]
        public string Team { get; set; }

        [StringLength(maximumLength: 255)]
        public string Comment { get; set; }

        public string Url { get; set; }

        public PhaseEnum Phase { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? FinishedDate { get; set; }
        public List<UserStory> UserStoryList { get; set; }
        public UserStory CurrentRunningUserStory { get; set; }
    }
}