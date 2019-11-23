using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CpsBoostAgile.BL;
using CpsBoostAgile.BL.API;
using CpsBoostAgile.Enumeration;
using CpsBoostAgile.Hubs.Containers;
using CpsBoostAgile.JsonObjects;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using WebGrease.Css.Extensions;

namespace CpsBoostAgile.Hubs
{
    public class PokerPlanningHub : Hub
    {
        private static UserContainer _userContainer;
        private IPokerPlanningService _pokerPlanningService;
        private IEventService _eventService;

        public PokerPlanningHub()
        {
            _pokerPlanningService = new PokerPlanningService();
            _eventService = new EventService();
            _userContainer = UserContainer.GetInstance;
        }

        #region User Management



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
            
            RestoreUserStories(eventId);

            if (eventState == PhaseEnum.Presentation)
            {
                var estimationsByUser = GetEstimations(eventId);
                string estimations = JsonConvert.SerializeObject(estimationsByUser);
                Clients.Caller.startPresentation(estimations);
            }
                
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

            var eventState = _eventService.GetCurrentPhaseOfEvent(eventId);
            if (eventState == PhaseEnum.Presentation)
            {
                var estimationsByUser = GetEstimations(eventId);
                string estimations = JsonConvert.SerializeObject(estimationsByUser);
                Clients.Caller.startPresentation(estimations);
            }
        }

        #endregion

        #region Workflow Management

        [Authorize]
        public void setToRunning(string storyId)
        {
            var connectionId = GetConnectionIdForUser();
            var ppId = _userContainer.GetEventId(connectionId);

            List<string> receivers = new List<string>();
            _userContainer.GetConnectionIdList(ppId, receivers);
            _pokerPlanningService.StartUserStory(storyId);
            var userStoryText = _pokerPlanningService.GetUserStory(storyId).Description;
            var userStoryComment = _pokerPlanningService.GetUserStory(storyId).Comment;
            _eventService.ChangePhase(ppId, Context.User.Identity.GetUserId(), PhaseEnum.Running);
            _userContainer.CleanPreviousEstimationResults(ppId);
            Clients.Clients(receivers).startUserStory(storyId, userStoryText, userStoryComment);
        }

        [Authorize]
        public void setToPresentation(string storyId)
        {
            var connectionId = GetConnectionIdForUser();
            var ppId = _userContainer.GetEventId(connectionId);

            List<string> receivers = new List<string>();
            _userContainer.GetConnectionIdList(ppId, receivers);

            _eventService.ChangePhase(ppId, Context.User.Identity.GetUserId(), PhaseEnum.Presentation);

            var estimationsByUser = GetEstimations(ppId).OrderBy(o=>o.Estimation).ToList();

            _userContainer.MarkAsNotReady(ppId);
            UpdateCurrentOnlineUserList(ppId);

            string estimations = JsonConvert.SerializeObject(estimationsByUser);
            Clients.Clients(receivers).startPresentation(estimations);
        }

        [Authorize]
        public void finishUserStoryEstimation(string userStoryId)
        {
            var connectionId = GetConnectionIdForUser();
            var ppId = _userContainer.GetEventId(connectionId);

            List<string> receivers = new List<string>();
            _userContainer.GetConnectionIdList(ppId, receivers);

            var estimationsByUser = GetEstimations(ppId);
            
            var finalEstimation =
                estimationsByUser.Where(w=>w.Estimation != 9999).OrderByDescending(w => w.Estimation).FirstOrDefault()?.Estimation;

            _pokerPlanningService.FinishUserStory(userStoryId, finalEstimation);
            _userContainer.CleanPreviousEstimationResults(ppId);

            _eventService.ChangePhase(ppId, Context.User.Identity.GetUserId(), PhaseEnum.Awaiting);
            Clients.Clients(receivers).switchBackToAwaiting();

            RestoreUserStories(ppId);
        }

        [Authorize]
        public void finishPokerPlanning()
        {
            var userId = Context.User.Identity.GetUserId();
            var connectionId = GetConnectionIdForUser();
            var id = _userContainer.GetEventId(connectionId);

            if (_eventService.ChangePhase(id, userId, PhaseEnum.Completed))
            {
                List<string> receivers = new List<string>();
                _userContainer.GetConnectionIdList(id, receivers);
                Clients.Clients(receivers).finishEvent();

                _userContainer.RemoveEventConnectionId(id);

                string noUsers = JsonConvert.SerializeObject(new List<string>());
                Clients.Clients(receivers).updateOnlineUsers(noUsers);
            }
        }

        [Authorize]
        public void setCoffeeBreak()
        {
            var connectionId = GetConnectionIdForUser();
            var id = _userContainer.GetEventId(connectionId);
            List<string> receivers = new List<string>();
            _userContainer.GetConnectionIdList(id, receivers);
            Clients.Clients(receivers).showCoffeeBreak();
        }

        [Authorize]
        public void finishCoffeeBreak()
        {
            var connectionId = GetConnectionIdForUser();
            var id = _userContainer.GetEventId(connectionId);
            List<string> receivers = new List<string>();
            _userContainer.GetConnectionIdList(id, receivers);
            Clients.Clients(receivers).continuePP();
        }

        #endregion

        public void assignEstimationFromUser(int estValue)
        {
            var connectionId = GetConnectionIdForUser();
            var eventId = _userContainer.GetEventId(connectionId);
            _userContainer.TryAssignEstimationCard(eventId, connectionId, estValue);
        }
        

        [Authorize]
        public void sendCommentToStory(string storyId, string commentOnStory)
        {
            var connectionId = GetConnectionIdForUser();
            var eventId = _userContainer.GetEventId(connectionId);
            if (_pokerPlanningService.AddCommentToStory(storyId, commentOnStory))
            {
                List<string> receivers = new List<string>();
                _userContainer.GetConnectionIdList(eventId, receivers);
                Clients.Clients(receivers).updateComment(commentOnStory);
            }
        }

        
        private string GetConnectionIdForUser()
        {
            string clientConnectionId = "";
            if (Context.QueryString["clientConnectionId"] != null)
                clientConnectionId = Context.QueryString["clientConnectionId"];

            if (string.IsNullOrEmpty(clientConnectionId.Trim()))
                clientConnectionId = Context.ConnectionId;

            return clientConnectionId;
        }

        private void UpdateCurrentOnlineUserList(string eventId)
        {
            List<OnlineUser> onlineUsers = new List<OnlineUser>();
            _userContainer.UpdateOnlineUserList(eventId, onlineUsers);
            string serializedObj = JsonConvert.SerializeObject(onlineUsers);

            List<string> receivers = new List<string>();
            _userContainer.GetConnectionIdList(eventId, receivers);

            Clients.Clients(receivers).updateOnlineUsers(serializedObj);
        }

        private void RestoreUserStories(string pokerPlanningId)
        {
            var stories = _pokerPlanningService.GetUserStories(pokerPlanningId).OrderBy(o=>o.CreatedDate).ToList();
            string serializedObj = JsonConvert.SerializeObject(stories);
            Clients.Caller.restoreStories(serializedObj);
        }

        private List<EstimationFromUsers> GetEstimations(string pokerPlanningId)
        {
            var estimationDic = _userContainer.GetPickedEstimationCards(pokerPlanningId);

            // transform input
            var estimationsByUser = new Dictionary<int, List<string>>();
            foreach (var est in estimationDic)
            {
                if (estimationsByUser.ContainsKey(est.Value))
                {
                    estimationsByUser[est.Value].Add(est.Key);
                }
                else
                {
                    estimationsByUser.Add(est.Value, new List<string> { est.Key });
                }
            }

            var estimationsFromUsers = new List<EstimationFromUsers>();
            estimationsByUser.ForEach(x => estimationsFromUsers.Add(new EstimationFromUsers { Estimation = x.Key, NumberOfVoters = x.Value.Count, Voters = x.Value }));
            return estimationsFromUsers;
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