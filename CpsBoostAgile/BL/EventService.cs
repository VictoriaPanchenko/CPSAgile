using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CpsBoostAgile.BL.API;
using CpsBoostAgile.DAO;
using CpsBoostAgile.DbContext;
using CpsBoostAgile.Enumeration;
using CpsBoostAgile.Models.Retrospective;

namespace CpsBoostAgile.BL
{
    public class EventService : IEventService
    {
        public string CreateEvent(RetrospectiveViewModels model, EventTypeEnum eventType, string userId)
        {
            using (var db = CpsContext.Create())
            {
                var dbEvent = new Event();

                dbEvent.Id = Guid.NewGuid().ToString();
                dbEvent.Name = model.RetrospectiveName;
                dbEvent.ProjectName = model.Project;
                dbEvent.Comment = model.Comment;
                dbEvent.TeamName = model.Team;
                dbEvent.Sprint = model.Sprint;
                dbEvent.CreatedBy = userId;
                dbEvent.StartedDate = DateTime.Now;
                dbEvent.Phase = PhaseEnum.Awaiting;
                dbEvent.EventType = eventType;

                if (eventType == EventTypeEnum.Retrospective)
                {
                    var dbRetrospective = new Retrospective
                    {
                        Id = dbEvent.Id,
                        EnableVoting = model.EnableVoting,
                        VotesPerUser = model.MaxVotesPerUser.GetValueOrDefault()
                    };

                    db.Retrospectives.Add(dbRetrospective);
                }
                
                db.Events.Add(dbEvent);
                db.SaveChanges();

                return dbEvent.Id;
            }
        }

        public bool DeleteEvent(string eventId)
        {
            using (var db = CpsContext.Create())
            {
                try
                {
                    var dbEvent = db.Events.SingleOrDefault(e => e.Id == eventId);

                    if (dbEvent != null)
                    {
                        if (dbEvent.EventType == EventTypeEnum.Retrospective)
                        {
                            var dbRetrospective = db.Retrospectives.SingleOrDefault(e => e.Id == eventId);
                            if (dbRetrospective != null)
                            {
                                var retroItems =
                                    db.RetrospectiveItems.Where(r => r.RetrospectiveId == dbRetrospective.Id);
                                db.RetrospectiveItems.RemoveRange(retroItems);
                                db.Retrospectives.Remove(dbRetrospective);
                            }
                        }

                        db.Events.Remove(dbEvent);
                        db.SaveChanges();

                        return true;
                    }

                    return false;
                }
                catch(Exception e)
                {
                    return false;
                }
            }
        }

        public bool ChangePhase(string eventId, string userId, PhaseEnum newPhase)
        {
            using (var db = CpsContext.Create())
            {
                var dbEvent = db.Events.SingleOrDefault(e => e.Id == eventId);

                if (dbEvent != null && userId == dbEvent.CreatedBy)
                {
                    dbEvent.Phase = newPhase;

                    if (newPhase == PhaseEnum.Completed)
                        dbEvent.FinishedDate = DateTime.Now;

                    if (dbEvent.EventType == EventTypeEnum.PokerPlanning)
                    {
                        var dbUserStories = db.UserStories.Where(w => w.PokerPlanningId == eventId && w.FinishedDate == null).ToList();
                        dbUserStories.ForEach(s=>s.FinishedDate=dbEvent.FinishedDate);
                    }

                    db.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public bool HasModeratorRightToManageEvent(string eventId, string userId)
        {
            using (var db = CpsContext.Create())
            {
                var dbEvent = db.Events
                    .SingleOrDefault(s => s.Id == eventId);

                return dbEvent != null && dbEvent.CreatedBy == userId;
            }
        }

        public PhaseEnum? GetCurrentPhaseOfEvent(string eventId)
        {
            using (var db = CpsContext.Create())
            {
                var eventDb = db.Events.SingleOrDefault(e => e.Id == eventId);

                return eventDb?.Phase;
            }
        }

        public List<Event> GetUsersEvents(string userId)
        {
            using (var db = CpsContext.Create())
            {
                var eventList = db.Events.Where(e => e.CreatedBy == userId).ToList();

                return eventList;
            }
        }

        public EventTypeEnum? GetEventType(string id)
        {
            using (var db = CpsContext.Create())
            {
                var eventType = db.Events.SingleOrDefault(e => e.Id == id)?.EventType;

                return eventType;
            }
        }
    }
}