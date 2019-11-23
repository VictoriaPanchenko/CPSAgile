using System.Collections.Generic;
using CpsBoostAgile.Models.Retrospective;

namespace CpsBoostAgile.Hubs.Containers
{
    public class UserContainer
    {
        private static readonly UserContainer _userContainerInstance = new UserContainer();

        private Dictionary<string, EventContainer> _eventContainers = new Dictionary<string, EventContainer>();

        private Dictionary<string, string> _connectionIdList = new Dictionary<string, string>();

        private UserContainer()
        {

        }

        public static UserContainer GetInstance
        {
            get
            {
                return _userContainerInstance;
            }
        }

        public void AddEventToContainer(string id)
        {
            if (!_eventContainers.ContainsKey(id))
                _eventContainers.Add(id, new EventContainer());
        }

        public void RemoveEventConnectionId(string id)
        {
            List<string> connectionIdList = new List<string>();
            GetConnectionIdList(id, connectionIdList);

            foreach (var conId in connectionIdList)
                _connectionIdList.Remove(conId);

            _eventContainers.Remove(id);
        }

        public bool AddModerator(string connectionId, string eventId)
        {
            if (!_eventContainers.ContainsKey(eventId))
                AddEventToContainer(eventId);

            EventContainer eventContainer;
            if (_eventContainers.TryGetValue(eventId, out eventContainer))
                eventContainer.AddModeratorToPool(connectionId);
            else
                return false;

            _connectionIdList.Add(connectionId, eventId);

            return true;
        }

        public bool AddRegularUser(string name, string connectionId, string eventId)
        {
            if (!_eventContainers.ContainsKey(eventId))
                AddEventToContainer(eventId);

            EventContainer eventContainer;
            if (_eventContainers.TryGetValue(eventId, out eventContainer))
                eventContainer.AddRegularUserToPool(name: name, connectionId: connectionId);
            else
                return false;

            _connectionIdList.Add(connectionId, eventId);

            return true;
        }

        public bool AssignUserNameAndColorToRetrospectiveItem(string connectionId, string retrospectiveId, RetrospectiveItemViewModels retrospectiveItem)
        {
            EventContainer eventContainer;
            if (!_eventContainers.TryGetValue(retrospectiveId, out eventContainer))
                return false;

            return eventContainer.AssignUserToRetrospectiveItem(connectionId, retrospectiveItem);
        }

        public string GetEventId(string connectionId)
        {
            string eventId;
            if (_connectionIdList.TryGetValue(connectionId, out eventId))
                return eventId;
            return null;
        }

        public string DeleteUser(string connectionId)
        {
            string eventId;
            if (_connectionIdList.TryGetValue(connectionId, out eventId))
                _connectionIdList.Remove(connectionId);
            else
                return null;

            EventContainer currentEvent;
            if (_eventContainers.TryGetValue(eventId, out currentEvent))
                currentEvent.DeleteUserFromPool(connectionId);

            return eventId;
        }

        public void MarkUserAsReady(string connectionId, string eventId, List<string> connectionIdList)
        {
            EventContainer currentEvent;
            if (!_eventContainers.TryGetValue(eventId, out currentEvent))
                return;

            currentEvent.SetAsReady(connectionId, connectionIdList);
        }

        public void MarkAsNotReady(string eventId)
        {
            EventContainer currentEvent;
            if (!_eventContainers.TryGetValue(eventId, out currentEvent))
                return;

            currentEvent.SetNotReady();
        }

        public bool IsUserReady(string connectionId, string eventId)
        {
            EventContainer currentEvent;
            if (!_eventContainers.TryGetValue(eventId, out currentEvent))
                return false;

            return currentEvent.CheckIfUserIsReady(connectionId);
        }

        public void UpdateOnlineUserList(string eventId, List<OnlineUser> onlineUsers)
        {
            EventContainer currentEvent;
            if (!_eventContainers.TryGetValue(eventId, out currentEvent))
                return;

            currentEvent.UpdateOnlineUserList(onlineUsers);
        }

        public void GetConnectionIdList(string eventId, List<string> receivers)
        {
            EventContainer currentEvent;
            if (!_eventContainers.TryGetValue(eventId, out currentEvent))
                return;

            currentEvent.UpdateConnectionIdList(receivers);
        }

        public bool TryAssignVote(string eventId, string connectionId, string retrospectiveItemId, int maxVotes)
        {
            EventContainer retrospective;
            if (!_eventContainers.TryGetValue(eventId, out retrospective))
                return false;

            return retrospective.AssignVote(connectionId, retrospectiveItemId, maxVotes);
        }

        public bool TryAssignEstimationCard(string eventId, string connectionId, int estimationCard)
        {
            EventContainer pokerPlanning;
            if (!_eventContainers.TryGetValue(eventId, out pokerPlanning))
                return false;

            return pokerPlanning.AssignEstimationCard(connectionId, estimationCard);
        }

        public Dictionary<string, int> GetPickedEstimationCards(string eventId)
        {
            EventContainer pokerPlanning;
            if (!_eventContainers.TryGetValue(eventId, out pokerPlanning))
                return null;

            return pokerPlanning.GetEstimations();
        }

        public void CleanPreviousEstimationResults(string eventId)
        {
            EventContainer pokerPlanning;
            if (!_eventContainers.TryGetValue(eventId, out pokerPlanning))
                return;

            pokerPlanning.CleanEstimations();
        }

        public void UpdateUsersAndVotes(string retrospectiveId, string retrospectiveItemId, List<VotesFromUser> votes)
        {
            EventContainer retrospective;
            if (!_eventContainers.TryGetValue(retrospectiveId, out retrospective))
                return;

            retrospective.UpdateUsersAndVotes(retrospectiveItemId, votes);
        }

        public List<RetrospectiveItemViewModels> RestoreAlreadyAssignedVotes(string eventId, string connectionId, List<RetrospectiveItemViewModels> allSharedItems, int maxVotes)
        {
            EventContainer retrospective;
            if (!_eventContainers.TryGetValue(eventId, out retrospective))
                return allSharedItems;

            return retrospective.RestoreAssignedVotesFromUser(allSharedItems, connectionId, maxVotes);
        }
    }

    public class User
    {
        public string Name { get; set; }

        public bool IsReadyToShare { get; set; }

        public int Color { get; set; }

        public Dictionary<string,int> PickedEstimationCardsFromAllUsers { get; set; }

        public HashSet<string> AssignedVotes = new HashSet<string>();
    }

    public class OnlineUser
    {
        public string Name { get; set; }

        public bool IsReadyToShare { get; set; }

        public int Color { get; set; }
    }

    public class VotesFromUser
    {
        public string ConnectionId { get; set; }

        public int TotalAssignedVotesFromOneUser { get; set; }

        public bool EnableVotingForItem { get; set; }
    }
}