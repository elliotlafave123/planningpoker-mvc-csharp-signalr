using Microsoft.EntityFrameworkCore;
using PlanningPoker.Data;
using PlanningPoker.Interfaces;
using PlanningPoker.Models;
using PlanningPoker.Services;

namespace PlanningPoker.Tests.Services
{
    public class GameServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly IGameService _gameService;

        public GameServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _gameService = new GameService(_context);
        }

        [Fact]
        public async Task CreateGameAsync_ShouldCreateGame()
        {
            // Arrange
            string gameName = "Test Game";
            bool hostIsVoter = true;

            // Act
            var game = await _gameService.CreateGameAsync(gameName);

            // Assert
            Assert.NotNull(game);
            Assert.Equal(gameName, game.Name);
            Assert.Equal(hostIsVoter, game.HostIsVoter);
            Assert.False(game.IsRoundActive);
            Assert.NotNull(game.GameLink);
        }

        [Fact]
        public async Task GetGameByLinkAsync_ShouldReturnGame()
        {
            // Arrange
            var game = await _gameService.CreateGameAsync("Test Game");

            // Act
            var retrievedGame = await _gameService.GetGameByLinkAsync(game.GameLink);

            // Assert
            Assert.NotNull(retrievedGame);
            Assert.Equal(game.GameLink, retrievedGame.GameLink);
        }

        [Fact]
        public async Task StartRoundAsync_ShouldStartRound_WhenHost()
        {
            // Arrange
            var game = await _gameService.CreateGameAsync("Test Game");
            var player = new Player { Name = "Host", IsHost = true, ConnectionId = "host-connection", GameId = game.Id };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Act
            await _gameService.StartRoundAsync(game.GameLink, "Round 1", "host-connection");
            var updatedGame = await _gameService.GetGameByLinkAsync(game.GameLink);

            // Assert
            Assert.True(updatedGame.IsRoundActive);
            Assert.Equal("Round 1", updatedGame.RoundName);
        }

        [Fact]
        public async Task StartRoundAsync_ShouldThrowException_WhenNotHost()
        {
            // Arrange
            var game = await _gameService.CreateGameAsync("Test Game");
            var player = new Player { Name = "Player", IsHost = false, ConnectionId = "player-connection", GameId = game.Id };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _gameService.StartRoundAsync(game.GameLink, "Round 1", "player-connection"));
        }

        [Fact]
        public async Task EndRoundAsync_ShouldEndRound_WhenHost()
        {
            // Arrange
            var game = await _gameService.CreateGameAsync("Test Game");
            var player = new Player { Name = "Host", IsHost = true, ConnectionId = "host-connection", GameId = game.Id };
            game.IsRoundActive = true;
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Act
            await _gameService.EndRoundAsync(game.GameLink, "host-connection");
            var updatedGame = await _gameService.GetGameByLinkAsync(game.GameLink);

            // Assert
            Assert.False(updatedGame.IsRoundActive);
        }

        [Fact]
        public async Task ResetVotesAsync_ShouldResetVotes_WhenHost()
        {
            // Arrange
            var game = await _gameService.CreateGameAsync("Test Game");
            var player = new Player { Name = "Host", IsHost = true, ConnectionId = "host-connection", GameId = game.Id };
            var vote = new Vote { GameId = game.Id, PlayerId = player.Id, Card = "5" };
            _context.Players.Add(player);
            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            // Act
            await _gameService.ResetVotesAsync(game.GameLink, "host-connection");
            var votes = _context.Votes.ToList();

            // Assert
            Assert.Empty(votes);
            var updatedGame = await _gameService.GetGameByLinkAsync(game.GameLink);
            Assert.False(updatedGame.IsRoundActive);
        }
    }
}
