using Microsoft.EntityFrameworkCore;
using PlanningPoker.Data;
using PlanningPoker.Interfaces;
using PlanningPoker.Models;

namespace PlanningPoker.Services
{
    public class GameService : IGameService
    {
        private readonly ApplicationDbContext _context;

        public GameService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Game> CreateGameAsync(string gameName, bool hostIsVoter)
        {
            var game = new Game
            {
                Name = gameName,
                HostIsVoter = hostIsVoter,
                GameLink = GenerateGameLink(),
                IsRoundActive = false,
                Players = new List<Player>(),
                Votes = new List<Vote>()
            };

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            return game;
        }

        public async Task<Game> GetGameByLinkAsync(string gameLink)
        {
            return await _context.Games
                .Include(g => g.Players)
                .Include(g => g.Votes)
                .FirstOrDefaultAsync(g => g.GameLink == gameLink);
        }

        public async Task StartRoundAsync(string gameLink, string roundName, string connectionId)
        {
            var game = await GetGameByLinkAsync(gameLink);
            if (game == null) throw new Exception("Game not found.");

            var player = game.Players.FirstOrDefault(p => p.ConnectionId == connectionId && p.IsHost);
            if (player == null) throw new Exception("Unauthorized.");

            game.IsRoundActive = true;
            game.RoundName = roundName;

            _context.Votes.RemoveRange(game.Votes);

            await _context.SaveChangesAsync();
        }

        public async Task EndRoundAsync(string gameLink, string connectionId)
        {
            var game = await GetGameByLinkAsync(gameLink);
            if (game == null) throw new Exception("Game not found.");

            var player = game.Players.FirstOrDefault(p => p.ConnectionId == connectionId && p.IsHost);
            if (player == null) throw new Exception("Unauthorized.");

            game.IsRoundActive = false;

            await _context.SaveChangesAsync();
        }

        public async Task ResetVotesAsync(string gameLink, string connectionId)
        {
            var game = await GetGameByLinkAsync(gameLink);
            if (game == null) throw new Exception("Game not found.");

            var player = game.Players.FirstOrDefault(p => p.ConnectionId == connectionId && p.IsHost);
            if (player == null) throw new Exception("Unauthorized.");

            _context.Votes.RemoveRange(game.Votes);
            game.IsRoundActive = false;

            await _context.SaveChangesAsync();
        }

        private string GenerateGameLink()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}