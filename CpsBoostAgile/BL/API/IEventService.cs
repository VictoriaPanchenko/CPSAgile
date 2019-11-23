using System.Collections.Generic;
using CpsBoostAgile.DAO;
using CpsBoostAgile.Enumeration;
using CpsBoostAgile.Models.Retrospective;

namespace CpsBoostAgile.BL.API
{
    public interface IEventService
    {
        string CreateEvent(RetrospectiveViewModels model, EventTypeEnum eventType, string createdBy);

        bool ChangePhase(string eventId, string userId, PhaseEnum newPhase);
        bool HasModeratorRightToManageEvent(string eventId, string userId);

        PhaseEnum? GetCurrentPhaseOfEvent(string eventId);

        bool DeleteEvent(string eventId);

        List<Event> GetUsersEvents(string userId);

        EventTypeEnum? GetEventType(string id);
    }
}