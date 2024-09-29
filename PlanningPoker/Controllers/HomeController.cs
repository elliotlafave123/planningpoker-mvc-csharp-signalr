using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PlanningPoker.Interfaces;

namespace PlanningPoker.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGameService _gameService;
        private readonly IPlayerService _playerService;
        private readonly bool _isDevelopment;
        private readonly string _requestScheme;

        public HomeController(IGameService gameService, IPlayerService playerService)
        {
            _gameService = gameService;
            _playerService = playerService;
            _isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            _requestScheme = _isDevelopment ? "http" : "https";
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateGame(string gameName)
        {
            var game = await _gameService.CreateGameAsync(gameName);
            return RedirectToAction("GameLobby", new { gameLink = game.GameLink });
        }

        public async Task<IActionResult> GameLobby(string gameLink)
        {
            var game = await _gameService.GetGameByLinkAsync(gameLink);
            if (game == null)
            {
                return NotFound("Game not found.");
            }

            game.GameLink = Url.Action("JoinGame", "Home", new { gameLink = game.GameLink }, _requestScheme);

            return View(game);
        }

        public async Task<IActionResult> JoinGame(string gameLink)
        {
            var game = await _gameService.GetGameByLinkAsync(gameLink);
            if (game == null)
            {
                return NotFound("Game not found.");
            }

            game.GameLink = Url.Action("JoinGame", "Home", new { gameLink = game.GameLink }, _requestScheme);

            return View(game);
        }
    }
}