using System.Collections.Generic;
using CpsBoostAgile.Enumeration;
using CpsBoostAgile.Models.Retrospective;

namespace CpsBoostAgile.BL.API
{
    public interface IRetrospectiveService
    {
        RetrospectiveViewModels GetRetrospective(string retrospectiveId);

        int GetMaxAllowedVotesPerUser(string retrospectiveId);

        List<RetrospectiveItemViewModels> GetRetrospectiveItems(string retrospectiveId);

        void SaveRetrospectiveItemToDb(RetrospectiveItemViewModels item);

        int AssignVoteToItemAndGetTotalVotes(string retrospectiveItemId);
    }
}
