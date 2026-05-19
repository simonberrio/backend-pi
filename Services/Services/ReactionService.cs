using AutoMapper;
using Dtos;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepositories;
using Repositories.Models;
using Repositories.Repositories;
using Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class ReactionService(IEventRepository eventRepository,
        IReactionRepository reactionRepository,
        IUserService userService) : IReactionService
    {
        private readonly IReactionRepository _reactionRepository = reactionRepository;
        private readonly IUserService _userService = userService;

        public async Task DeleteReaction(int eventId)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            Event @event = await eventRepository.GetQueryable().Where(x => x.Id == eventId).FirstOrDefaultAsync() ??
                throw new Exception("Error al buscar el evento para eliminar la reacción.");

            Reaction? reaction = await _reactionRepository.GetQueryable().Where(x => x.EventId == eventId && x.UserId.Equals(user.Id)).FirstOrDefaultAsync() ??
                throw new Exception("No ha reaccionado a este evento.");

            await _reactionRepository.DeleteAsync(reaction);
        }

        public async Task<ReactionSummaryDto> GetReactionsByEventId(int eventId)
        {
            List<Reaction> reactions = await _reactionRepository.GetQueryable().Where(x => x.EventId == eventId).ToListAsync();
            return new ReactionSummaryDto
            {
                Like = reactions.Count(r => r.Type == ReactionTypeEnums.Like),
                Love = reactions.Count(r => r.Type == ReactionTypeEnums.Love),
                Laugh = reactions.Count(r => r.Type == ReactionTypeEnums.Laugh),
                Wow = reactions.Count(r => r.Type == ReactionTypeEnums.Wow),
                Sad = reactions.Count(r => r.Type == ReactionTypeEnums.Sad)
            };
        }

        public async Task ReactToEvent(int eventId, ReactionTypeEnums reactionTypeId)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            Event @event = await eventRepository.GetQueryable().Where(x => x.Id == eventId).FirstOrDefaultAsync() ??
                throw new Exception("Error al buscar el evento para reaccionar.");

            Reaction? reaction = await _reactionRepository.GetQueryable().Where(x => x.EventId == eventId && x.UserId.Equals(user.Id)).FirstOrDefaultAsync();

            if (reaction == null)
            {
                reaction = new Reaction
                {
                    EventId = eventId,
                    UserId = user.Id,
                    Type = reactionTypeId,
                    CreatedDate = DateTime.UtcNow
                };

                await _reactionRepository.CreateAsync(reaction);
            }
            else
            {
                reaction.Type = reactionTypeId;

                await _reactionRepository.UpdateAsync(reaction);
            }
        }
    }
}
