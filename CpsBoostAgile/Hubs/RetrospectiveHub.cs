using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CpsBoostAgile.BL;
using CpsBoostAgile.BL.API;
using System.Threading.Tasks;
using CpsBoostAgile.Enumeration;
using CpsBoostAgile.Hubs.Containers;
using CpsBoostAgile.Models.Retrospective;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace CpsBoostAgile.Hubs
{
    public class RetrospectiveHub : Hub
    {
        private static UserContainer _userContainer;
        private IRetrospectiveService _retrospectiveService;
        private IEventService _eventService;

        public RetrospectiveHub()
        {
            _retrospectiveService = new RetrospectiveService();
            _eventService = new EventService();
            _userContainer = UserContainer.GetInstance;
        }

        #region Phase Managment

        [Authorize]
        public void setRetrospectiveToRunning()
        {
            var userConnectionId = GetConnectionIdForUser();
            var eventId = _userContainer.GetEventId(userConnectionId);

            if (eventId != null && _eventService.ChangePhase(eventId, Context.User.Identity.GetUserId(), PhaseEnum.Running))
            {
                List<string> receivers = new List<string>();
                _userContainer.GetConnectionIdList(eventId, receivers);
                Clients.Clients(receivers).setRetrospectiveRunningPhase();
            }
        }

        [Authorize]
        public void setRetrospectiveToPresentation()
        {
            var userId = Context.User.Identity.GetUserId();
            var userConnectionId = GetConnectionIdForUser();
            var eventId = _userContainer.GetEventId(userConnectionId);

            if (eventId != null &&
                _eventService.ChangePhase(eventId, userId, PhaseEnum.Presentation))
            {
                List<string> receivers = new List<string>();
                _userContainer.GetConnectionIdList(eventId, receivers);
                Clients.Clients(receivers).setRetrospectivePresentationPhase();

                _userContainer.MarkAsNotReady(eventId);
                UpdateCurrentOnlineUserList(eventId);
            }
        }

        [Authorize]
        public void setRetrospectiveToVoting()
        {
            var userId = Context.User.Identity.GetUserId();
            var userConnectionId = GetConnectionIdForUser();
            var eventId = _userContainer.GetEventId(userConnectionId);

            if (eventId != null &&
                _eventService.ChangePhase(eventId, userId, PhaseEnum.Voting))
            {
                List<string> receivers = new List<string>();
                _userContainer.GetConnectionIdList(eventId, receivers);
                Clients.Clients(receivers).setRetrospectiveVotingPhase();
            }
        }

        [Authorize]
        public void finishEvent()
        {
            var userId = Context.User.Identity.GetUserId();
            var userConnectionId = GetConnectionIdForUser();
            var eventId = _userContainer.GetEventId(userConnectionId);

            if (eventId != null &&
                _eventService.ChangePhase(eventId, userId, PhaseEnum.Completed))
            {
                List<string> receivers = new List<string>();
                _userContainer.GetConnectionIdList(eventId, receivers);
                Clients.Clients(receivers).finishRetrospectiveEvent();

                _userContainer.RemoveEventConnectionId(eventId);

                string noUsers = JsonConvert.SerializeObject(new List<string>());
                Clients.Clients(receivers).updateOnlineUsers(noUsers);
            }
        }

        #endregion

        #region User Management

        [Authorize]
        public void addModeratorToPool(string eventId)
        {
            var connectionId = GetConnectionIdForUser();
            var userId = Context.User.Identity.GetUserId();
            var hasRight = _eventService.HasModeratorRightToManageEvent(eventId, userId);

            if (!hasRight)
                return;

            _userContainer.AddModerator(connectionId, eventId);
            UpdateCurrentOnlineUserList(eventId);
            
            var eventState = _eventService.GetCurrentPhaseOfEvent(eventId);
            switch (eventState)
            {
                case PhaseEnum.Awaiting:
                    return;
                case PhaseEnum.Running:
                    Clients.Caller.setRetrospectiveRunningPhase();
                    if (_userContainer.IsUserReady(connectionId, eventId))
                        Clients.Caller.setReadyMode();
                    return;
                case PhaseEnum.Presentation:
                    Clients.Caller.setRetrospectivePresentationPhase();
                    break;
                case PhaseEnum.Voting:
                    Clients.Caller.setRetrospectiveVotingPhase();
                    break;
                case PhaseEnum.Completed:
                    Clients.Caller.finishRetrospectiveEvent();
                    break;
                default:
                    return;
            }

            RestoreSharedItems(eventId, connectionId, eventState.GetValueOrDefault());
        }


        public void addRegularUserToPool(string user, string eventId)
        {
            var connectionId = GetConnectionIdForUser();

            var currentEventState = _eventService.GetCurrentPhaseOfEvent(eventId);
            if (currentEventState == null)
                return;
            
            var validationMessage = ValidateRegularUserLogin(user);
            if (validationMessage != null)
            {
                Clients.Caller.invalidLoginName(validationMessage);
                return;
            }

            _userContainer.AddRegularUser(user, connectionId, eventId);
            UpdateCurrentOnlineUserList(eventId);
            
            Clients.Caller.userConnected(user);

            switch (currentEventState)
            {
                case PhaseEnum.Awaiting:
                    return;
                case PhaseEnum.Running:
                    Clients.Caller.setRetrospectiveRunningPhase();
                    if (_userContainer.IsUserReady(connectionId, eventId))
                        Clients.Caller.setReadyMode();
                    return;
                case PhaseEnum.Presentation:
                    Clients.Caller.setRetrospectivePresentationPhase();
                    break;
                case PhaseEnum.Voting:
                    Clients.Caller.setRetrospectiveVotingPhase();
                    break;
                case PhaseEnum.Completed:
                    Clients.Caller.finishRetrospectiveEvent();
                    break;
                default:
                    return;
            }

            RestoreSharedItems(eventId, connectionId, currentEventState.GetValueOrDefault());
        }

        public void setUserReady()
        {
            var connectionId = GetConnectionIdForUser();
            var eventId = _userContainer.GetEventId(connectionId);

            if (eventId != null)
            {
                if (_eventService.GetCurrentPhaseOfEvent(eventId) == PhaseEnum.Running)
                {
                    List<string> connectionIds = new List<string>();

                    _userContainer.MarkUserAsReady(connectionId, eventId, connectionIds);

                    if (connectionIds.Count > 0)
                    {
                        foreach (var user in connectionIds)
                            Clients.Client(user).switchToReady();

                        UpdateCurrentOnlineUserList(eventId);
                    }
                }
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var eventId = _userContainer.DeleteUser(GetConnectionIdForUser());

            if (!string.IsNullOrWhiteSpace(eventId))
                UpdateCurrentOnlineUserList(eventId);

            return base.OnDisconnected(stopCalled);
        }
        #endregion

        #region Event Managment

        public void restoreItemsAfterEvent(string eventId)
        {
            var connectionId = GetConnectionIdForUser();
            RestoreSharedItems(eventId,connectionId,PhaseEnum.Completed);
        }

        public void shareRetrospectiveItem(RetrospectiveColumnTypeEnum groupType, string text)
        {
            if (string.IsNullOrWhiteSpace(text)
                || (groupType != RetrospectiveColumnTypeEnum.Start && groupType != RetrospectiveColumnTypeEnum.Stop && groupType != RetrospectiveColumnTypeEnum.Continue))
                return;

            var connectionId = GetConnectionIdForUser();
            var eventId = _userContainer.GetEventId(connectionId);

            if (eventId == null)
                return;

            var phase = _eventService.GetCurrentPhaseOfEvent(eventId);
            if (phase == null || phase != PhaseEnum.Presentation)
                return;

            var enableVoting = _retrospectiveService.GetRetrospective(eventId).EnableVoting;
            var retrospectiveItem = new RetrospectiveItemViewModels { Id = Guid.NewGuid().ToString(), RetrospectiveId = eventId, Group = groupType, Text = text, IsEnabledForVoting = enableVoting };

            if (!_userContainer.AssignUserNameAndColorToRetrospectiveItem(connectionId, eventId, retrospectiveItem))
                return;

            _retrospectiveService.SaveRetrospectiveItemToDb(retrospectiveItem); // tut neobjayatelno color ukladat
            var item = JsonConvert.SerializeObject(retrospectiveItem);

            List<string> receivers = new List<string>();
            _userContainer.GetConnectionIdList(eventId, receivers);
            Clients.Clients(receivers).receiveNewSharedItem(item);
        }

        public void addVoteToSharedItem(string itemId)
        {
            var connectionId = GetConnectionIdForUser();
            var eventId = _userContainer.GetEventId(connectionId);

            if (eventId == null)
                return;

            var maxVotes = _retrospectiveService.GetMaxAllowedVotesPerUser(eventId);
            if (maxVotes > 0)
            {
                var retrospective = _retrospectiveService.GetRetrospective(eventId);
                if (retrospective != null && _userContainer.TryAssignVote(eventId, connectionId, itemId, maxVotes))
                {
                    var totalRating = _retrospectiveService.AssignVoteToItemAndGetTotalVotes(itemId);
                    List<VotesFromUser> votes = new List<VotesFromUser>();
                    _userContainer.UpdateUsersAndVotes(eventId, itemId, votes);

                    foreach (var vote in votes)
                    {
                        
                        var model = new RetrospectiveItemViewModels() { Id = itemId, TotalAssignedVotes = totalRating, RemainedVotesForParticularUser = maxVotes - vote.TotalAssignedVotesFromOneUser, IsEnabledForVoting = vote.EnableVotingForItem };
                        var item = JsonConvert.SerializeObject(model);

                        Clients.Client(vote.ConnectionId).assignVoteToSharedItem(item);
                    }
                }
            }
        }
        #endregion

        private string GetConnectionIdForUser()
        {
            string clientConnectionId = "";
            if (Context.QueryString["clientConnectionId"] != null)
                clientConnectionId = Context.QueryString["clientConnectionId"];

            if (string.IsNullOrEmpty(clientConnectionId.Trim()))
                clientConnectionId = Context.ConnectionId;

            return clientConnectionId;
        }

        private void UpdateCurrentOnlineUserList(string guidId)
        {
            List<OnlineUser> onlineUsers = new List<OnlineUser>();
            _userContainer.UpdateOnlineUserList(guidId, onlineUsers);
            string serializedObj = JsonConvert.SerializeObject(onlineUsers);

            List<string> receivers = new List<string>();
            _userContainer.GetConnectionIdList(guidId, receivers);

            Clients.Clients(receivers).updateOnlineUsers(serializedObj);
        }

        private void RestoreSharedItems(string eventId, string connectionId, PhaseEnum eventPhase)
        {
            var allRetrospectiveItems = _retrospectiveService.GetRetrospectiveItems(eventId).OrderBy(o=>o.CreatedDate).ToList();
            allRetrospectiveItems.ForEach(rItem=> _userContainer.AssignUserNameAndColorToRetrospectiveItem(connectionId, eventId, rItem));
            var sharedItems = new List<RetrospectiveItemViewModels>();
            sharedItems.AddRange(allRetrospectiveItems);
            var maxAllowedVotes = _retrospectiveService.GetMaxAllowedVotesPerUser(eventId);

            if (maxAllowedVotes > 0)
            {
                if (eventPhase == PhaseEnum.Voting || eventPhase == PhaseEnum.Completed)
                    _userContainer.RestoreAlreadyAssignedVotes(eventId, connectionId, sharedItems, maxAllowedVotes);
            }

            string item = JsonConvert.SerializeObject(sharedItems);

            Clients.Caller.recieveSharedItems(item);
        }

        private string ValidateRegularUserLogin(string userLogin)
        {
            if (string.IsNullOrWhiteSpace(userLogin))
                return "'Name' cannot be empty!";

            if (userLogin.Length > 20)
                return "Maximum length of 'Name' is 20 characters!";

            if (userLogin.Any(char.IsDigit) || userLogin.Any(char.IsSymbol))
                return "Numbers and symbols are not allowed!";

            return null;
        }
    }
}