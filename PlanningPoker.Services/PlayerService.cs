using Microsoft.EntityFrameworkCore;
using PlanningPoker.Data;
using PlanningPoker.Interfaces;
using PlanningPoker.Models;

namespace PlanningPoker.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly ApplicationDbContext _context;

        public PlayerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Player> JoinGameAsync(string gameLink, string playerName, string connectionId)
        {
            var game = await _context.Games
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.GameLink == gameLink);

            if (game == null)
                throw new Exception("Game not found.");

            int maxPlayers = game.HostIsVoter ? 10 : 9;
            if (game.Players.Count >= maxPlayers)
                throw new Exception("Game is full.");

            var existingPlayer = game.Players.FirstOrDefault(p => p.Name == playerName);
            if (existingPlayer != null)
            {
                existingPlayer.ConnectionId = connectionId;
                await _context.SaveChangesAsync();
                return existingPlayer;
            }
            else
            {
                var player = new Player
                {
                    ConnectionId = connectionId,
                    Name = playerName,
                    GameId = game.Id,
                    IsHost = false
                };

                _context.Players.Add(player);
                await _context.SaveChangesAsync();
                return player;
            }
        }

        public async Task<Player> GetPlayerByConnectionIdAsync(string connectionId)
        {
            return await _context.Players
                .Include(p => p.Game)
                .FirstOrDefaultAsync(p => p.ConnectionId == connectionId);
        }

        public async Task RemovePlayerAsync(string connectionId)
        {
            var player = await GetPlayerByConnectionIdAsync(connectionId);
            if (player != null)
            {
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Player>> GetPlayersInGameAsync(string gameLink)
        {
            var game = await _context.Games
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.GameLink == gameLink);

            return game?.Players.ToList() ?? new List<Player>();
        }
    }
}