using System;
using System.Collections.Generic;
using System.Linq;
using CpsBoostAgile.BL.API;
using CpsBoostAgile.DAO;
using CpsBoostAgile.DbContext;
using CpsBoostAgile.Enumeration;
using CpsBoostAgile.Models.PokerPlanning;
using CpsBoostAgile.Models.Retrospective;

namespace CpsBoostAgile.BL
{
    public class PokerPlanningService : IPokerPlanningService
    {
        public string CreatePokerPlanningEvent(PPViewModels model, EventTypeEnum eventType, string createdBy)
        {
            using (var db = CpsContext.Create())
            {
                var dbEvent = new Event();

                dbEvent.Id = Guid.NewGuid().ToString();
                dbEvent.Name = model.EventName;
                dbEvent.ProjectName = model.Project;
                dbEvent.Comment = model.Comment;
                dbEvent.TeamName = model.Team;
                dbEvent.Sprint = model.Sprint;
                dbEvent.CreatedBy = createdBy;
                dbEvent.StartedDate = DateTime.Now;
                dbEvent.Phase = PhaseEnum.Awaiting;
                dbEvent.EventType = eventType;

                db.Events.Add(dbEvent);
                db.SaveChanges();

                return dbEvent.Id;
            }
        }

        public PPViewModels GetPokerPlanningEvent(string id)
        {
            using (var db = CpsContext.Create())
            {
                var eventDb = db.Events.SingleOrDefault(e => e.Id == id);

                var pokerPlanningEvent = new PPViewModels();

                if (eventDb != null)
                {
                    pokerPlanningEvent.Id = eventDb.Id;
                    pokerPlanningEvent.EventName = eventDb.Name;
                    pokerPlanningEvent.Phase = eventDb.Phase;
                    pokerPlanningEvent.Project = eventDb.ProjectName;
                    pokerPlanningEvent.Sprint = eventDb.Sprint;
                    pokerPlanningEvent.Team = eventDb.TeamName;
                    pokerPlanningEvent.Comment = eventDb.Comment;
                    pokerPlanningEvent.CreatedBy = eventDb.CreatedBy;
                    pokerPlanningEvent.CreatedDate = eventDb.StartedDate;
                    pokerPlanningEvent.FinishedDate = eventDb.FinishedDate;
                    pokerPlanningEvent.CurrentRunningUserStory =
                        db.UserStories.FirstOrDefault(w => w.PokerPlanningId==id && w.StartedDate != null && w.FinishedDate == null);
                    pokerPlanningEvent.UserStoryList = db.UserStories.Where(
                                    w => w.PokerPlanningId == id).ToList();
                }

                return pokerPlanningEvent;
            }
        }

        public string SaveUserStory(string text, string ppEventId)
        {
            using (var db = CpsContext.Create())
            {
                var userStory = new UserStory
                {
                    Id = Guid.NewGuid().ToString(),
                    PokerPlanningId = ppEventId,
                    Description = text,
                    CreatedDate = DateTime.Now
                };

                db.UserStories.Add(userStory);
                db.SaveChanges();

                return userStory.Id;
            }
        }

        public bool DeleteUserStory(string id)
        {
            using (var db = CpsContext.Create())
            {
                var userStory = db.UserStories.SingleOrDefault(s=>s.Id == id);

                if (userStory != null)
                {
                    db.UserStories.Remove(userStory);
                    db.SaveChanges();

                    return true;
                }

                return false;
            }
        }

        public bool StartUserStory(string id)
        {
            using (var db = CpsContext.Create())
            {
                var userStory = db.UserStories.SingleOrDefault(s => s.Id == id);

                if (userStory != null)
                {
                    userStory.StartedDate = DateTime.Now;
                    
                    db.SaveChanges();

                    return true;
                }

                return false;
            }
        }

        public UserStory GetUserStory(string id)
        {
            using (var db = CpsContext.Create())
            {
                var userStoryDb = db.UserStories.SingleOrDefault(s => s.Id == id);

                return userStoryDb;
            }
        }

        public bool AddCommentToStory(string id, string commentOnStory)
        {
            using (var db = CpsContext.Create())
            {
                var userStory = db.UserStories.SingleOrDefault(s => s.Id == id);

                if (userStory != null)
                {
                    userStory.Comment = commentOnStory;

                    db.SaveChanges();

                    return true;
                }

                return false;
            }
        }
        
        public List<UserStory> GetUserStories(string pokerPlanningId)
        {
            using (var db = CpsContext.Create())
            {
                var userStoryList = db.UserStories.Where(w=>w.PokerPlanningId == pokerPlanningId).ToList();

                return userStoryList;
            }
        }

        public bool FinishUserStory(string userStoryId, int? finalEstimation)
        {
            using (var db = CpsContext.Create())
            {
                var userStory = db.UserStories.SingleOrDefault(s => s.Id == userStoryId);
                if (userStory != null)
                {
                    userStory.FinishedDate = DateTime.Now;
                    userStory.FinalEstimation = finalEstimation;

                    db.SaveChanges();

                    return true;
                }

                return false;
            }
        }
    }
}
