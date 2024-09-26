using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Data;
using PlanningPoker.Models;

namespace PlanningPoker.Hubs
{
    public class GameHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public GameHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetConnectionId()
        {
            return Context.ConnectionId;
        }

        public async Task JoinGame(string gameLink, string playerName)
        {
            var game = _context.Games
                .Include(g => g.Players)
                .FirstOrDefault(g => g.GameLink == gameLink);
            if (game == null)
            {
                await Clients.Caller.SendAsync("Error", "Game not found.");
                return;
            }

            int maxPlayers = game.HostIsVoter ? 10 : 9;
            if (game.Players.Count >= maxPlayers)
            {
                await Clients.Caller.SendAsync("Error", "Game is full.");
                return;
            }

            // Check if the player already exists by name
            var existingPlayer = game.Players.FirstOrDefault(p => p.Name == playerName);
            if (existingPlayer != null)
            {
                existingPlayer.ConnectionId = Context.ConnectionId;
                await _context.SaveChangesAsync();
            }
            else
            {
                var player = new Player
                {
                    ConnectionId = Context.ConnectionId,
                    Name = playerName,
                    GameId = game.Id,
                    IsHost = false
                };

                _context.Players.Add(player);
                await _context.SaveChangesAsync();
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, gameLink);

            var players = game.Players.Select(p => new { p.Name }).ToList();
            await Clients.Group(gameLink).SendAsync("UpdatePlayerList", players);
        }

        public async Task JoinGameAsHost(string gameLink)
        {
            var game = _context.Games
                .Include(g => g.Players)
                .FirstOrDefault(g => g.GameLink == gameLink);
            if (game == null)
            {
                await Clients.Caller.SendAsync("Error", "Game not found.");
                return;
            }

            var hostPlayer = game.Players.FirstOrDefault(p => p.IsHost);
            if (hostPlayer == null)
            {
                var player = new Player
                {
                    ConnectionId = Context.ConnectionId,
                    Name = "Host",
                    GameId = game.Id,
                    IsHost = true
                };

                _context.Players.Add(player);
            }
            else
            {
                hostPlayer.ConnectionId = Context.ConnectionId;
            }

            await _context.SaveChangesAsync();

            await Groups.AddToGroupAsync(Context.ConnectionId, gameLink);

            var players = game.Players.Select(p => new { p.Name }).ToList();
            await Clients.Group(gameLink).SendAsync("UpdatePlayerList", players);
        }
        public async Task SubmitVote(string gameLink, string cardValue)
        {
            var game = _context.Games
                .Include(g => g.Players)
                .Include(g => g.Votes)
                .FirstOrDefault(g => g.GameLink == gameLink);

            if (game == null || !game.IsRoundActive)
            {
                await Clients.Caller.SendAsync("Error", "No active round to vote on.");
                return;
            }

            var player = game.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("Error", "Player not found in the game.");
                return;
            }

            // Check if the player has already voted
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

            // Notify clients that the player has voted
            await Clients.Group(gameLink).SendAsync("PlayerVoted", player.Name);

            // Check if all players have voted
            var totalPlayers = game.Players.Count;
            var totalVotes = game.Votes.Count;

            if (totalVotes >= totalPlayers)
            {
                game.IsRoundActive = false;
                await _context.SaveChangesAsync();

                // Reveal votes to all clients
                var votes = game.Votes
                    .Select(v => new { playerName = v.Player.Name, card = v.Card })
                    .ToList();

                await Clients.Group(gameLink).SendAsync("VotesRevealed", votes);
            }
        }

        public async Task StartRound(string gameLink, string roundName)
        {
            var game = _context.Games
                .Include(g => g.Players)
                .FirstOrDefault(g => g.GameLink == gameLink);

            if (game != null)
            {
                var player = game.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (player != null && player.IsHost)
                {
                    game.IsRoundActive = true;
                    game.RoundName = roundName;
                    _context.Votes.RemoveRange(game.Votes);
                    await _context.SaveChangesAsync();

                    await Clients.Group(gameLink).SendAsync("RoundStarted", roundName);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "You are not authorized to start the round.");
                }
            }
        }

        public async Task EndRound(string gameLink)
        {
            var game = _context.Games
                .Include(g => g.Players)
                .Include(g => g.Votes)
                .FirstOrDefault(g => g.GameLink == gameLink);

            if (game != null)
            {
                var player = game.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (player != null && player.IsHost)
                {
                    game.IsRoundActive = false;
                    await _context.SaveChangesAsync();

                    // Reveal votes to all clients
                    var votes = game.Votes
                        .Select(v => new { playerName = v.Player.Name, card = v.Card })
                        .ToList();

                    await Clients.Group(gameLink).SendAsync("VotesRevealed", votes);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "You are not authorized to end the round.");
                }
            }
        }

        public async Task ResetVotes(string gameLink)
        {
            var game = _context.Games
                .Include(g => g.Players)
                .Include(g => g.Votes)
                .FirstOrDefault(g => g.GameLink == gameLink);

            if (game != null)
            {
                var player = game.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (player != null && player.IsHost)
                {
                    _context.Votes.RemoveRange(game.Votes);
                    game.IsRoundActive = false;
                    await _context.SaveChangesAsync();

                    // Notify clients to reset votes
                    await Clients.Group(gameLink).SendAsync("VotesReset");
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "You are not authorized to reset votes.");
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var player = _context.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player != null)
            {
                var gameLink = player.Game.GameLink;
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();

                var game = _context.Games
                    .Include(g => g.Players)
                    .FirstOrDefault(g => g.GameLink == gameLink);

                if (game != null)
                {
                    var players = game.Players.Select(p => new { p.Name }).ToList();
                    await Clients.Group(gameLink).SendAsync("UpdatePlayerList", players);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
