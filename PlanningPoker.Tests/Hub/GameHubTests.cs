using Microsoft.AspNetCore.SignalR;
using Moq;
using PlanningPoker.Hubs;
using PlanningPoker.Interfaces;
using PlanningPoker.Models;

namespace PlanningPoker.Tests.Hubs
{
    public class GameHubTests
    {
        private readonly Mock<IGameService> _mockGameService;
        private readonly Mock<IPlayerService> _mockPlayerService;
        private readonly Mock<IVoteService> _mockVoteService;
        private readonly GameHub _hub;
        private readonly Mock<HubCallerContext> _mockContext;
        private readonly Mock<IHubCallerClients> _mockClients;
        private readonly Mock<IClientProxy> _mockClientProxy;

        public GameHubTests()
        {
            _mockGameService = new Mock<IGameService>();
            _mockPlayerService = new Mock<IPlayerService>();
            _mockVoteService = new Mock<IVoteService>();

            _mockContext = new Mock<HubCallerContext>();
            _mockClients = new Mock<IHubCallerClients>();
            _mockClientProxy = new Mock<IClientProxy>();

            _hub = new GameHub(_mockGameService.Object, _mockPlayerService.Object, _mockVoteService.Object)
            {
                Context = _mockContext.Object,
                Clients = _mockClients.Object
            };
        }

        [Fact]
        public async Task JoinGame_ShouldAddPlayerAndNotifyGroup()
        {
            // Arrange
            string gameLink = "testlink";
            string playerName = "Player1";
            string connectionId = "conn1";
            _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

            var player = new Player { Name = playerName };
            _mockPlayerService.Setup(s => s.JoinGameAsync(gameLink, playerName, connectionId)).ReturnsAsync(player);
            _mockPlayerService.Setup(s => s.GetPlayersInGameAsync(gameLink)).ReturnsAsync(new List<Player> { player });

            _mockClients.Setup(c => c.Group(gameLink)).Returns(_mockClientProxy.Object);

            // Act
            await _hub.JoinGame(gameLink, playerName);

            // Assert
            _mockClients.Verify(c => c.Group(gameLink), Times.Once);
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "UpdatePlayerList",
                    It.Is<object[]>(o => ((IEnumerable<Player>)o[0]).First().Name == playerName),
                    default(CancellationToken)),
                Times.Once);
        }

        [Fact]
        public async Task SubmitVote_ShouldNotifyPlayerVoted_AndRevealVotes_WhenAllVoted()
        {
            // Arrange
            string gameLink = "testlink";
            string connectionId = "conn1";
            _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

            var player = new Player { Name = "Player1", ConnectionId = connectionId };
            var players = new List<Player> { player };
            var votes = new List<Vote> { new() { Player = player, Card = "5" } };

            _mockPlayerService.Setup(s => s.GetPlayerByConnectionIdAsync(connectionId)).ReturnsAsync(player);
            _mockPlayerService.Setup(s => s.GetPlayersInGameAsync(gameLink)).ReturnsAsync(players);
            _mockVoteService.Setup(s => s.GetVotesInGameAsync(gameLink)).ReturnsAsync(votes);
            _mockClients.Setup(c => c.Group(gameLink)).Returns(_mockClientProxy.Object);

            // Act
            await _hub.SubmitVote(gameLink, "5");

            // Assert
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "PlayerVoted",
                    It.Is<object[]>(o => (string)o[0] == "Player1"),
                    default(CancellationToken)),
                Times.Once);

            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "VotesRevealed",
                    It.IsAny<object[]>(),
                    default(CancellationToken)),
                Times.Once);
        }

        [Fact]
        public async Task StartRound_ShouldNotifyRoundStarted_WhenHost()
        {
            // Arrange
            string gameLink = "testlink";
            string connectionId = "host-conn";
            _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

            _mockGameService.Setup(s => s.StartRoundAsync(gameLink, "Round 1", connectionId)).Returns(Task.CompletedTask);
            _mockClients.Setup(c => c.Group(gameLink)).Returns(_mockClientProxy.Object);

            // Act
            await _hub.StartRound(gameLink, "Round 1");

            // Assert
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "RoundStarted",
                    It.Is<object[]>(o => (string)o[0] == "Round 1"),
                    default(CancellationToken)),
                Times.Once);
        }

        [Fact]
        public async Task EndRound_ShouldRevealVotes_WhenHost()
        {
            // Arrange
            string gameLink = "testlink";
            string connectionId = "host-conn";
            _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

            var votes = new List<Vote>
            {
                new() { Player = new Player { Name = "Player1" }, Card = "5" },
                new() { Player = new Player { Name = "Player2" }, Card = "8" }
            };

            _mockGameService.Setup(s => s.EndRoundAsync(gameLink, connectionId)).Returns(Task.CompletedTask);
            _mockVoteService.Setup(s => s.GetVotesInGameAsync(gameLink)).ReturnsAsync(votes);
            _mockClients.Setup(c => c.Group(gameLink)).Returns(_mockClientProxy.Object);

            // Act
            await _hub.EndRound(gameLink);

            // Assert
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "VotesRevealed",
                    It.Is<object[]>(o => ((IEnumerable<dynamic>)o[0]).Count() == 2),
                    default(CancellationToken)),
                Times.Once);
        }

        [Fact]
        public async Task OnDisconnectedAsync_ShouldRemovePlayerAndNotifyGroup()
        {
            // Arrange
            string connectionId = "conn1";
            _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

            var player = new Player { Name = "Player1", Game = new Game { GameLink = "testlink" } };
            var players = new List<Player>();

            _mockPlayerService.Setup(s => s.GetPlayerByConnectionIdAsync(connectionId)).ReturnsAsync(player);
            _mockPlayerService.Setup(s => s.RemovePlayerAsync(connectionId)).Returns(Task.CompletedTask);
            _mockPlayerService.Setup(s => s.GetPlayersInGameAsync("testlink")).ReturnsAsync(players);
            _mockClients.Setup(c => c.Group("testlink")).Returns(_mockClientProxy.Object);

            // Act
            await _hub.OnDisconnectedAsync(null);

            // Assert
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "UpdatePlayerList",
                    It.Is<object[]>(o => ((IEnumerable<dynamic>)o[0]).Count() == 0),
                    default(CancellationToken)),
                Times.Once);
        }
    }
}
