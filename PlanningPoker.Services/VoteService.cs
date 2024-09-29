using Microsoft.EntityFrameworkCore;
using PlanningPoker.Data;
using PlanningPoker.Interfaces;
using PlanningPoker.Models;

namespace PlanningPoker.Services
{
    public class VoteService : IVoteService
    {
        private readonly ApplicationDbContext _context;

        public VoteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SubmitVoteAsync(string gameLink, string cardValue, string connectionId)
        {
            var game = await _context.Games
                .Include(g => g.Players)
                .Include(g => g.Votes)
                .FirstOrDefaultAsync(g => g.GameLink == gameLink);

            if (game == null || !game.IsRoundActive)
                throw new Exception("No active round to vote on.");

            var player = game.Players.FirstOrDefault(p => p.ConnectionId == connectionId);
            if (player == null)
                throw new Exception("Player not found in the game.");

            var existingVote = game.Votes.FirstOrDefault(v => v.PlayerId == player.Id);
            if (existingVote != null)
            {
                existingVote.Card = cardValue;
            }
            else
            {
                var vote = new Vote
                {
                    Card = cardValue,
                    PlayerId = player.Id,
                    GameId = game.Id
                };
                _context.Votes.Add(vote);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Vote>> GetVotesInGameAsync(string gameLink)
        {
            var game = await _context.Games
                .Include(g => g.Votes)
                .ThenInclude(v => v.Player)
                .FirstOrDefaultAsync(g => g.GameLink == gameLink);

            return game?.Votes.ToList() ?? new List<Vote>();
        }

        public async Task ResetVotesAsync(string gameLink)
        {
            var game = await _context.Games
                .Include(g => g.Votes)
                .FirstOrDefaultAsync(g => g.GameLink == gameLink);

            if (game != null)
            {
                _context.Votes.RemoveRange(game.Votes);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasPlayerVotedAsync(string gameLink, int playerId)
        {
            return await _context.Votes.AnyAsync(v => v.Game.GameLink == gameLink && v.PlayerId == playerId);
        }
    }
}