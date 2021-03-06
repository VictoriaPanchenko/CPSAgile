﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using CpsBoostAgile.DAO;
using CpsBoostAgile.Enumeration;

namespace CpsBoostAgile.Models.Retrospective
{
    public class RetrospectiveViewModels
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Name*")]
        [StringLength(maximumLength: 30)]
        public string RetrospectiveName { get; set; }

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

        [Display(Name = "Enable Voting phase")]
        public bool EnableVoting { get; set; }

        [Display(Name = "Maximum Votes per participant")]
        public int? MaxVotesPerUser { get; set; }

        public string Url { get; set; }

        public PhaseEnum Phase { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? FinishedDate { get; set; }

        public List<RetrospectiveItem> RetrospectiveItems { get; set; }
        
    }
}