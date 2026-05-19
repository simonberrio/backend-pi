using Dtos;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IService
{
    public interface IReactionService
    {
        Task DeleteReaction(int eventId);
        Task<ReactionSummaryDto> GetReactionsByEventId(int eventId);
        Task ReactToEvent(int eventId, ReactionTypeEnums reactionTypeId);
    }
}
