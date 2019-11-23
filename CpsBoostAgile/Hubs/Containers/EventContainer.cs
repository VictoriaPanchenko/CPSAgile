using System.Collections.Generic;
using System.Linq;
using CpsBoostAgile.Enumeration;
using CpsBoostAgile.Models.Retrospective;

namespace CpsBoostAgile.Hubs.Containers
{
    public class EventContainer
    {
        private HashSet<string> _moderator;
        private HashSet<string> _votesFromModerator;
        private Dictionary<string, User> _users;
        private Dictionary<string, int> _pickedEstimationCards;
        private string _moderatorName;
        private bool _moderatorReady;
        private int _moderatorColor;
        private int _lastColor;

        public EventContainer()
        {
            _moderator = new HashSet<string>();
            _votesFromModerator = new HashSet<string>();
            _users = new Dictionary<string, User>();
            _pickedEstimationCards = new Dictionary<string, int>();
            _moderatorName = "Event Moderator";
            _lastColor = 0;
        }

        public void AddModeratorToPool(string connectionId)
        {
            _moderator.Add(connectionId);
            if (_moderator.Count == 1)
                _moderatorColor = AssignColor();
        }

        public void AddRegularUserToPool(string name, string connectionId)
        {
            var user = new User { Name = name, PickedEstimationCardsFromAllUsers = new Dictionary<string, int>(), AssignedVotes = new HashSet<string>(), Color = AssignColor() };
            _users.Add(key: connectionId, value: user);
        }

        public bool AssignUserToRetrospectiveItem(string connectionId, RetrospectiveItemViewModels item)
        {
            // restore colors for already ones shared retrospective items 
            if (item.CreatedBy != null)
            {
                if (item.CreatedBy == _moderatorName)
                {
                    item.Color = _moderatorColor;
                    return true;
                }
                else
                {
                    var color = _users.Values.FirstOrDefault(x => x.Name == item.CreatedBy)?.Color;
                    item.Color = color.GetValueOrDefault();
                    return true;
                }
            }

            if (_moderator.Contains(connectionId))
            {
                item.CreatedBy = _moderatorName;
                item.Color = _moderatorColor;
                return true;
            }

            User user;
            if (_users.TryGetValue(connectionId, out user))
            {
                item.CreatedBy = user.Name;
                item.Color = user.Color;
                return true;
            }
            return false;
        }

        public void DeleteUserFromPool(string connectionId)
        {
            _moderator.Remove(connectionId);
            _users.Remove(connectionId);
        }

        public void SetAsReady(string connectionId, List<string> connectionIdList)
        {
            if (_moderator.Contains(connectionId))
            {
                _moderatorReady = true;
                foreach (var connection in _moderator)
                    connectionIdList.Add(connection);
            }
            else
            {
                User user;
                if (!_users.TryGetValue(connectionId, out user))
                    return;

                user.IsReadyToShare = true;
                connectionIdList.Add(connectionId);
            }
        }

        public void SetNotReady()
        {
            _moderatorReady = false;
            foreach (var user in _users)
                user.Value.IsReadyToShare = false;
        }

        public bool CheckIfUserIsReady(string connectionId)
        {
            if (_moderator.Contains(connectionId))
                return _moderatorReady;
            else
            {
                User user;
                if (!_users.TryGetValue(connectionId, out user))
                    return false;
                return user.IsReadyToShare;
            }
        }

        
        public void UpdateOnlineUserList(List<OnlineUser> userList)
        {
            if (_moderator.Count > 0)
                userList.Add(new OnlineUser() { Name = _moderatorName, IsReadyToShare = _moderatorReady, Color = _moderatorColor });

            foreach (var item in _users)
                userList.Add(new OnlineUser() { Name = item.Value.Name, IsReadyToShare = item.Value.IsReadyToShare, Color = item.Value.Color });
        }

        public void UpdateConnectionIdList(List<string> connections)
        {
            foreach (var connection in _moderator)
                connections.Add(connection);

            foreach (var item in _users)
                connections.Add(item.Key);
        }

        public bool AssignVote(string connectionId, string retrospectiveItemId, int maxVotes)
        {
            if (_moderator.Contains(connectionId))
            {
                if (!_votesFromModerator.Contains(retrospectiveItemId) && _votesFromModerator.Count < maxVotes)
                {
                    _votesFromModerator.Add(retrospectiveItemId);
                    return true;
                }
            }
            else
            {
                User user;
                if (!_users.TryGetValue(connectionId, out user))
                    return false;

                if (!user.AssignedVotes.Contains(retrospectiveItemId) && user.AssignedVotes.Count < maxVotes)
                {
                    user.AssignedVotes.Add(retrospectiveItemId);
                    return true;
                }
            }
            return false;
        }

        public bool AssignEstimationCard(string connectionId, int estimationCard)
        {
            if (_moderator.Contains(connectionId))
            {
                if (!_pickedEstimationCards.ContainsKey(_moderatorName))
                {
                    _pickedEstimationCards.Add(_moderatorName, estimationCard);
                    return true;
                }
                else
                {
                    _pickedEstimationCards[_moderatorName] = estimationCard;
                    return true;
                }
            }
            else
            {
                User user;
                if (!_users.TryGetValue(connectionId, out user))
                    return false;

                if (!_pickedEstimationCards.ContainsKey(user.Name))
                {
                    _pickedEstimationCards.Add(user.Name, estimationCard);
                    return true;
                }
                else
                {
                    _pickedEstimationCards[user.Name] = estimationCard;
                    return true;
                }
            }
        }

        public Dictionary<string, int> GetEstimations()
        {
            return _pickedEstimationCards;
        }

        public void CleanEstimations()
        {
            _pickedEstimationCards.Clear();
        }

        public void UpdateUsersAndVotes(string sharedItemGuid, List<VotesFromUser> votesFromUser)
        {
            foreach (var connection in _moderator)
                votesFromUser.Add(new VotesFromUser { ConnectionId = connection, TotalAssignedVotesFromOneUser = _votesFromModerator.Count, EnableVotingForItem = !_votesFromModerator.Contains(sharedItemGuid) });
            foreach (var user in _users)
                votesFromUser.Add(new VotesFromUser { ConnectionId = user.Key, TotalAssignedVotesFromOneUser = user.Value.AssignedVotes.Count, EnableVotingForItem = !user.Value.AssignedVotes.Contains(sharedItemGuid) });
        }

       
        public List<RetrospectiveItemViewModels> RestoreAssignedVotesFromUser(List<RetrospectiveItemViewModels> sharedItems, string connectionId, int maxAllowedVotes)
        {
            if (!_moderator.Contains(connectionId))
                return sharedItems;

            foreach (var item in sharedItems)
            {
                if (_votesFromModerator.Contains(item.Id))
                    item.IsEnabledForVoting = false;

                // restore used votes
                item.RemainedVotesForParticularUser = maxAllowedVotes - _votesFromModerator.Count;
            }

            return sharedItems;
        }

        private int AssignColor()
        {
            // Number of unique colors
            if (_lastColor >= 19)
            {
                _lastColor = 0;
                return _lastColor;
            }
            else
                return _lastColor++;
        }
    }
}