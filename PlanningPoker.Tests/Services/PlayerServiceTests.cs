using Microsoft.EntityFrameworkCore;
using PlanningPoker.Data;
using PlanningPoker.Interfaces;
using PlanningPoker.Models;
using PlanningPoker.Services;

namespace PlanningPoker.Tests.Services
{
    public class PlayerServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly IPlayerService _playerService;
        private readonly IGameService _gameService;

        public PlayerServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _playerService = new PlayerService(_context);
            _gameService = new GameService(_context);
        }

        [Fact]
        public async Task JoinGameAsync_ShouldAddPlayer()
        {
            // Arrange
            var game = await _gameService.CreateGameAsync("Test Game");
            string playerName = "Player1";
            string connectionId = "conn1";

            // Act
            var player = await _playerService.JoinGameAsync(game.GameLink, playerName, connectionId);

            // Assert
            Assert.NotNull(player);
            Assert.Equal(playerName, player.Name);
            Assert.Equal(connectionId, player.ConnectionId);
            Assert.False(player.IsHost);
        }

        [Fact]
        public async Task JoinGameAsync_ShouldUpdateConnectionId_IfPlayerExists()
        {
            // Arrange
            var game = await _gameService.CreateGameAsync("Test Game");
            string playerName = "Player1";
            string connectionId1 = "conn1";
            string connectionId2 = "conn2";

            // Act
            await _playerService.JoinGameAsync(game.GameLink, playerName, connectionId1);
            var player = await _playerService.JoinGameAsync(game.GameLink, playerName, connectionId2);

            // Assert
            Assert.NotNull(player);
            Assert.Equal(connectionId2, player.ConnectionId);
        }

        [Fact]
        public async Task JoinGameAsync_ShouldThrowException_IfGameFull()
        {
            // Arrange
            var game = await _gameService.CreateGameAsync("Test Game");
            for (int i = 0; i < 9; i++)
            {
                _context.Players.Add(new Player { Name = $"Player{i}", GameId = game.Id });
            }
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _playerService.JoinGameAsync(game.GameLink, "NewPlayer", "new-conn"));
        }

        [Fact]
        public async Task GetPlayerByConnectionIdAsync_ShouldReturnPlayer()
        {
            // Arrange
            var game = await _gameService.CreateGameAsync("Test Game");
            var player = new Player { Name = "Player1", ConnectionId = "conn1", GameId = game.Id };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Act
            var retrievedPlayer = await _playerService.GetPlayerByConnectionIdAsync("conn1");

            // Assert
            Assert.NotNull(retrievedPlayer);
            Assert.Equal("Player1", retrievedPlayer.Name);
        }

        [Fact]
        public async Task RemovePlayerAsync_ShouldRemovePlayer()
        {
            // Arrange
            var game = await _gameService.CreateGameAsync("Test Game");
            var player = new Player { Name = "Player1", ConnectionId = "conn1", GameId = game.Id };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Act
            await _playerService.RemovePlayerAsync("conn1");
            var retrievedPlayer = await _playerService.GetPlayerByConnectionIdAsync("conn1");

            // Assert
            Assert.Null(retrievedPlayer);
        }

        [Fact]
        public async Task GetPlayersInGameAsync_ShouldReturnPlayers()
        {
            // Arrange
            var game = await _gameService.CreateGameAsync("Test Game");
            var player1 = new Player { Name = "Player1", ConnectionId = "conn1", GameId = game.Id };
            var player2 = new Player { Name = "Player2", ConnectionId = "conn2", GameId = game.Id };
            _context.Players.AddRange(player1, player2);
            await _context.SaveChangesAsync();

            // Act
            var players = await _playerService.GetPlayersInGameAsync(game.GameLink);

            // Assert
            Assert.Equal(2, players.Count);
            Assert.Contains(players, p => p.Name == "Player1");
            Assert.Contains(players, p => p.Name == "Player2");
        }
    }
}
