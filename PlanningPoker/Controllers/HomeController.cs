using Microsoft.AspNetCore.Mvc;
using PlanningPoker.Interfaces;

namespace PlanningPoker.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGameService _gameService;
        private readonly IPlayerService _playerService;

        public HomeController(IGameService gameService, IPlayerService playerService)
        {
            _gameService = gameService;
            _playerService = playerService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateGame(string gameName, bool hostIsVoter)
        {
            var game = await _gameService.CreateGameAsync(gameName, hostIsVoter);
            return RedirectToAction("GameLobby", new { gameLink = game.GameLink });
        }

        public async Task<IActionResult> GameLobby(string gameLink)
        {
            var game = await _gameService.GetGameByLinkAsync(gameLink);
            if (game == null)
            {
                return NotFound("Game not found.");
            }

            return View(game);
        }

        public async Task<IActionResult> JoinGame(string gameLink)
        {
            var game = await _gameService.GetGameByLinkAsync(gameLink);
            if (game == null)
            {
                return NotFound("Game not found.");
            }

            return View(game);
        }
    }
}