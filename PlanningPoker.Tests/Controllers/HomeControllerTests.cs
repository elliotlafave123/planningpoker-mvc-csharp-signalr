using Microsoft.AspNetCore.Mvc;
using Moq;
using PlanningPoker.Controllers;
using PlanningPoker.Interfaces;
using PlanningPoker.Models;

namespace PlanningPoker.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly Mock<IGameService> _mockGameService;
        private readonly Mock<IPlayerService> _mockPlayerService;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _mockGameService = new Mock<IGameService>();
            _mockPlayerService = new Mock<IPlayerService>();
            _controller = new HomeController(_mockGameService.Object, _mockPlayerService.Object);
        }

        [Fact]
        public void Index_ShouldReturnView()
        {
            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateGame_ShouldRedirectToGameLobby()
        {
            // Arrange
            string gameName = "Test Game";
            bool hostIsVoter = true;
            var game = new Game { GameLink = "testlink" };
            _mockGameService.Setup(s => s.CreateGameAsync(gameName, hostIsVoter)).ReturnsAsync(game);

            // Act
            var result = await _controller.CreateGame(gameName, hostIsVoter) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("GameLobby", result.ActionName);
            Assert.Equal("testlink", result.RouteValues["gameLink"]);
        }

        [Fact]
        public async Task GameLobby_ShouldReturnView_WhenGameExists()
        {
            // Arrange
            var game = new Game { GameLink = "testlink" };
            _mockGameService.Setup(s => s.GetGameByLinkAsync("testlink")).ReturnsAsync(game);

            // Act
            var result = await _controller.GameLobby("testlink") as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(game, result.Model);
        }

        [Fact]
        public async Task GameLobby_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            // Arrange
            _mockGameService.Setup(s => s.GetGameByLinkAsync("testlink")).ReturnsAsync((Game)null);

            // Act
            var result = await _controller.GameLobby("testlink") as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Game not found.", result.Value);
        }

        [Fact]
        public async Task JoinGame_ShouldReturnView_WhenGameExists()
        {
            // Arrange
            var game = new Game { GameLink = "testlink" };
            _mockGameService.Setup(s => s.GetGameByLinkAsync("testlink")).ReturnsAsync(game);

            // Act
            var result = await _controller.JoinGame("testlink") as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(game, result.Model);
        }

        [Fact]
        public async Task JoinGame_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            // Arrange
            _mockGameService.Setup(s => s.GetGameByLinkAsync("testlink")).ReturnsAsync((Game)null);

            // Act
            var result = await _controller.JoinGame("testlink") as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Game not found.", result.Value);
        }
    }
}
