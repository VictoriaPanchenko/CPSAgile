using System;
using System.Collections.Generic;
using System.Linq;
using CpsBoostAgile.BL.API;
using CpsBoostAgile.DAO;
using CpsBoostAgile.DbContext;
using CpsBoostAgile.Enumeration;
using CpsBoostAgile.Models.Retrospective;

namespace CpsBoostAgile.BL
{
    public class RetrospectiveService : IRetrospectiveService
    {
        public RetrospectiveViewModels GetRetrospective(string id)
        {
            using (var db = CpsContext.Create())
            {
                var eventDb = db.Events.SingleOrDefault(e => e.Id == id);
                var retrospectiveConfig = db.Retrospectives.SingleOrDefault(e => e.Id == id);

                var retrospective = new RetrospectiveViewModels();

                if(eventDb != null && retrospectiveConfig != null)
                {
                    retrospective.Id = eventDb.Id;
                    retrospective.RetrospectiveName = eventDb.Name;
                    retrospective.Phase = eventDb.Phase;
                    retrospective.Project = eventDb.ProjectName;
                    retrospective.Sprint = eventDb.Sprint;
                    retrospective.Team = eventDb.TeamName;
                    retrospective.Comment = eventDb.Comment;
                    retrospective.EnableVoting = retrospectiveConfig.EnableVoting;
                    retrospective.MaxVotesPerUser = retrospectiveConfig.VotesPerUser;
                    retrospective.CreatedBy = eventDb.CreatedBy;
                    retrospective.CreatedDate = eventDb.StartedDate;
                    retrospective.FinishedDate = eventDb.FinishedDate;
                    retrospective.RetrospectiveItems =
                        db.RetrospectiveItems.Where(w => w.RetrospectiveId == id).ToList();
                }
                
                return retrospective;
            }
        }

        public int GetMaxAllowedVotesPerUser(string retrospectiveId)
        {
            using (var db = CpsContext.Create())
            {
                var dbItem = db.Retrospectives
                    .SingleOrDefault(s => s.Id == retrospectiveId);

                return dbItem?.VotesPerUser ?? 0;
            }
        }

        
        public List<RetrospectiveItemViewModels> GetRetrospectiveItems(string retrospectiveId)
        {
            using (var db = CpsContext.Create())
            {
                var enableVoting = db.Retrospectives.FirstOrDefault(w => w.Id == retrospectiveId)?.EnableVoting;
                var retrospectiveItems = db.RetrospectiveItems
                    .Where(w => w.RetrospectiveId == retrospectiveId)
                    .Select(s => new RetrospectiveItemViewModels()
                    {
                        RetrospectiveId = s.RetrospectiveId,
                        CreatedBy = s.CreatedBy,
                        Group = s.Group,
                        Text = s.Text,
                        CreatedDate = s.CreatedDate,
                        Id = s.Id,
                        TotalAssignedVotes = s.Rating,
                        IsEnabledForVoting = enableVoting ?? false
                    }).ToList();

                return retrospectiveItems;
            }
        }

        public void SaveRetrospectiveItemToDb(RetrospectiveItemViewModels item)
        {
            using (var db = CpsContext.Create())
            {
                var dbRetrospectiveItem = new RetrospectiveItem
                {
                    Id = item.Id,
                    RetrospectiveId = item.RetrospectiveId,
                    Group = item.Group,
                    CreatedBy = item.CreatedBy,
                    CreatedDate = DateTime.Now,
                    Text = item.Text,
                    Rating = item.TotalAssignedVotes
                };
                
                db.RetrospectiveItems.Add(dbRetrospectiveItem);
                db.SaveChanges();
            }
        }

        public int AssignVoteToItemAndGetTotalVotes(string retrospectiveItemId)
        {
            using (var db = CpsContext.Create())
            {
                var dbRetrospectiveItem = db.RetrospectiveItems.SingleOrDefault(i => i.Id == retrospectiveItemId);

                dbRetrospectiveItem.Rating++;
                db.SaveChanges();

                return dbRetrospectiveItem.Rating;
            }
        }
    }
}