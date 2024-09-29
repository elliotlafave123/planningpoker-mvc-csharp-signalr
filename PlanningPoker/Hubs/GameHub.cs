using Microsoft.AspNetCore.SignalR;
using PlanningPoker.Interfaces;

namespace PlanningPoker.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameService _gameService;
        private readonly IPlayerService _playerService;
        private readonly IVoteService _voteService;

        public GameHub(IGameService gameService, IPlayerService playerService, IVoteService voteService)
        {
            _gameService = gameService;
            _playerService = playerService;
            _voteService = voteService;
        }

        public async Task<string> GetConnectionId()
        {
            return Context.ConnectionId;
        }

        public async Task JoinGame(string gameLink, string playerName)
        {
            Console.WriteLine($"JoinGame called with gameLink: {gameLink}, playerName: {playerName}");
            try
            {
                await _playerService.JoinGameAsync(gameLink, playerName, Context.ConnectionId);
                await Groups.AddToGroupAsync(Context.ConnectionId, gameLink);

                var players = await _playerService.GetPlayersInGameAsync(gameLink);
                var playerNames = players
                    .Where(p => p.Name != "Host")
                    .Select(p => new { p.Name })
                    .ToList();

                await Clients.Group(gameLink).SendAsync("UpdatePlayerList", playerNames);
                Console.WriteLine("Player list sent to group:", playerNames);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task JoinGameAsHost(string gameLink)
        {
            Console.WriteLine($"JoinGameAsHost called with gameLink: {gameLink}");
            try
            {
                await _playerService.JoinGameAsync(gameLink, "Host", Context.ConnectionId);
                await Groups.AddToGroupAsync(Context.ConnectionId, gameLink);

                var players = await _playerService.GetPlayersInGameAsync(gameLink);
                var playerNames = players
                    .Where(p => p.Name != "Host")
                    .Select(p => new { p.Name })
                    .ToList();

                await Clients.Group(gameLink).SendAsync("UpdatePlayerList", playerNames);
                Console.WriteLine("Player list sent to group:", playerNames);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task SubmitVote(string gameLink, string cardValue)
        {
            try
            {
                await _voteService.SubmitVoteAsync(gameLink, cardValue, Context.ConnectionId);

                var player = await _playerService.GetPlayerByConnectionIdAsync(Context.ConnectionId);

                // Notify clients that the player has voted
                await Clients.Group(gameLink).SendAsync("PlayerVoted", player.Name);

                // Check if all players have voted
                var players = await _playerService.GetPlayersInGameAsync(gameLink);
                var votes = await _voteService.GetVotesInGameAsync(gameLink);

                if (votes.Count >= players.Count)
                {
                    await _gameService.EndRoundAsync(gameLink, Context.ConnectionId);

                    var voteResults = votes.Select(v => new { playerName = v.Player.Name, card = v.Card }).ToList();
                    await Clients.Group(gameLink).SendAsync("VotesRevealed", voteResults);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task StartRound(string gameLink, string roundName)
        {
            try
            {
                await _gameService.StartRoundAsync(gameLink, roundName, Context.ConnectionId);
                await Clients.Group(gameLink).SendAsync("RoundStarted", roundName);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task EndRound(string gameLink)
        {
            try
            {
                await _gameService.EndRoundAsync(gameLink, Context.ConnectionId);

                var votes = await _voteService.GetVotesInGameAsync(gameLink);
                var voteResults = votes.Select(v => new { playerName = v.Player.Name, card = v.Card }).ToList();

                await Clients.Group(gameLink).SendAsync("VotesRevealed", voteResults);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task GetGameState(string gameLink)
        {
            var game = await _gameService.GetGameByLinkAsync(gameLink);
            if (game != null)
            {
                var isRoundActive = game.IsRoundActive;
                var roundName = game.RoundName;

                // Get the list of votes if the round has ended
                List<object> voteResults = null;
                if (!isRoundActive)
                {
                    var votes = await _voteService.GetVotesInGameAsync(gameLink);
                    voteResults = votes.Select(v => new { playerName = v.Player.Name, card = v.Card }).ToList<object>();
                }

                // Send the game state to the caller
                await Clients.Caller.SendAsync("ReceiveGameState", new
                {
                    isRoundActive,
                    roundName,
                    votesRevealed = voteResults != null,
                    votes = voteResults
                });
            }
            else
            {
                // Handle the case where the game doesn't exist
                await Clients.Caller.SendAsync("Error", "Game not found.");
            }
        }

        public async Task GetPlayerGameState(string gameLink)
        {
            var game = await _gameService.GetGameByLinkAsync(gameLink);
            var player = await _playerService.GetPlayerByConnectionIdAsync(Context.ConnectionId);

            if (game != null && player != null)
            {
                var isRoundActive = game.IsRoundActive;
                var roundName = game.RoundName;

                // Check if the player has already voted
                var hasVoted = await _voteService.HasPlayerVotedAsync(gameLink, player.Id);

                // Get the list of votes if the round has ended
                List<object> voteResults = null;
                if (!isRoundActive)
                {
                    var votes = await _voteService.GetVotesInGameAsync(gameLink);
                    voteResults = votes.Select(v => new { playerName = v.Player.Name, card = v.Card }).ToList<object>();
                }

                // Send the game state to the caller
                await Clients.Caller.SendAsync("ReceivePlayerGameState", new
                {
                    isRoundActive,
                    roundName,
                    hasVoted,
                    votesRevealed = voteResults != null,
                    votes = voteResults
                });
            }
            else
            {
                await Clients.Caller.SendAsync("Error", "Game or player not found.");
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var player = await _playerService.GetPlayerByConnectionIdAsync(Context.ConnectionId);
            if (player != null)
            {
                var gameLink = player.Game.GameLink;
                await _playerService.RemovePlayerAsync(Context.ConnectionId);

                var players = await _playerService.GetPlayersInGameAsync(gameLink);
                var playerNames = players.Select(p => new { p.Name }).ToList();

                await Clients.Group(gameLink).SendAsync("UpdatePlayerList", playerNames);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}