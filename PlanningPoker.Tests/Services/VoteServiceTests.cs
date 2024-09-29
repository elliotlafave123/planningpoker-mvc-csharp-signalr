using Microsoft.EntityFrameworkCore;
using PlanningPoker.Data;
using PlanningPoker.Interfaces;
using PlanningPoker.Models;
using PlanningPoker.Services;

namespace PlanningPoker.Tests.Services
{
    public class VoteServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly IVoteService _voteService;
        private readonly IGameService _gameService;
        private readonly IPlayerService _playerService;

        public VoteServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _voteService = new VoteService(_context);
            _gameService = new GameService(_context);
            _playerService = new PlayerService(_context);
        }

        [Fact]
        public async Task SubmitVoteAsync_ShouldAddVote()
        {
            // Arrange
            var game = await _gameService.CreateGameAsync("Test Game");
            game.IsRoundActive = true;
            await _context.SaveChangesAsync();

            var player = new Player { Name = "Player1", ConnectionId = "conn1", GameId = game.Id };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Act
            await _voteService.SubmitVoteAsync(game.GameLink, "5", "conn1");

            // Assert
            var votes = await _context.Votes.ToListAsync();
            Assert.Single(votes);
            Assert.Equal("5", votes.First().Card);
        }

        [Fact]
        public async Task SubmitVoteAsync_ShouldUpdateVote_IfAlreadyExists()
        {
            // Arrange
            var game = await _gameService.CreateGameAsync("Test Game");
            game.IsRoundActive = true;
            await _context.SaveChangesAsync();

            var player = new Player { Name = "Player1", ConnectionId = "conn1", GameId = game.Id };
            var vote = new Vote { GameId = game.Id, PlayerId = player.Id, Card = "3" };
            _context.Players.Add(player);
            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            // Act
            await _voteService.SubmitVoteAsync(game.GameLink, "5", "conn1");

            // Assert
            var votes = await _context.Votes.ToListAsync();
            Assert.Single(votes);
            Assert.Equal("5", votes.First().Card);
        }

        [Fact]
        public async Task SubmitVoteAsync_ShouldThrowException_IfNoActiveRound()
        {
            // Arrange
            var game = await _gameService.CreateGameAsync("Test Game");
            var player = new Player { Name = "Player1", ConnectionId = "conn1", GameId = game.Id };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _voteService.SubmitVoteAsync(game.GameLink, "5", "conn1"));
        }

        [Fact]
        public async Task GetVotesInGameAsync_ShouldReturnVotes()
        {
            // Arrange
            var game = await _gameService.CreateGameAsync("Test Game");
            var player1 = new Player { Name = "Player1", GameId = game.Id };
            var player2 = new Player { Name = "Player2", GameId = game.Id };
            var vote1 = new Vote { GameId = game.Id, PlayerId = player1.Id, Card = "3" };
            var vote2 = new Vote { GameId = game.Id, PlayerId = player2.Id, Card = "5" };
            _context.Players.AddRange(player1, player2);
            _context.Votes.AddRange(vote1, vote2);
            await _context.SaveChangesAsync();

            // Act
            var votes = await _voteService.GetVotesInGameAsync(game.GameLink);

            // Assert
            Assert.Equal(2, votes.Count);
            Assert.Contains(votes, v => v.Card == "3");
            Assert.Contains(votes, v => v.Card == "5");
        }

        [Fact]
        public async Task ResetVotesAsync_ShouldRemoveAllVotes()
        {
            // Arrange
            var game = await _gameService.CreateGameAsync("Test Game");
            var vote = new Vote { GameId = game.Id, Card = "5" };
            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            // Act
            await _voteService.ResetVotesAsync(game.GameLink);

            // Assert
            var votes = await _context.Votes.ToListAsync();
            Assert.Empty(votes);
        }
    }
}
