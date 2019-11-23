using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CpsBoostAgile.DAO;
using CpsBoostAgile.Enumeration;
using CpsBoostAgile.Models.PokerPlanning;

namespace CpsBoostAgile.BL.API
{
    public interface IPokerPlanningService
    {
        string CreatePokerPlanningEvent(PPViewModels model, EventTypeEnum eventType, string createdBy);

        PPViewModels GetPokerPlanningEvent(string id);

        string SaveUserStory(string text, string ppEventId);

        bool DeleteUserStory(string id);

        bool StartUserStory(string id);
        UserStory GetUserStory(string id);

        List<UserStory> GetUserStories(string pokerPlanningId);

        bool AddCommentToStory(string id, string commentOnStory);

        bool FinishUserStory(string userStoryId, int? finalEstimation);
    }
}
