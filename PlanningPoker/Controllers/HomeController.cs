using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PlanningPoker.Data;
using PlanningPoker.Hubs;
using PlanningPoker.Models;

namespace PlanningPoker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<GameHub> _hubContext;

        public HomeController(ApplicationDbContext context, IHubContext<GameHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateGame(string gameName, bool hostIsVoter)
        {
            var game = new Game
            {
                Name = gameName,
                HostIsVoter = hostIsVoter,
                GameLink = GenerateGameLink(),
                IsRoundActive = false,
                Players = new System.Collections.Generic.List<Player>(),
                Votes = new System.Collections.Generic.List<Vote>()
            };

            _context.Games.Add(game);
            _context.SaveChanges();

            return RedirectToAction("GameLobby", new { gameLink = game.GameLink });
        }

        public IActionResult GameLobby(string gameLink)
        {
            var game = _context.Games
                .Include(g => g.Players)
                .FirstOrDefault(g => g.GameLink == gameLink);

            if (game == null)
            {
                return NotFound("Game not found.");
            }

            return View(game);
        }

        public IActionResult JoinGame(string gameLink)
        {
            var game = _context.Games.FirstOrDefault(g => g.GameLink == gameLink);
            if (game == null)
            {
                return NotFound("Game not found.");
            }

            return View(game);
        }

        [HttpPost]
        public IActionResult StartRound(string gameLink)
        {
            var game = _context.Games.FirstOrDefault(g => g.GameLink == gameLink);
            if (game != null)
            {
                // Verify that the requester is the host
                var hostPlayer = game.Players.FirstOrDefault(p => p.IsHost && p.ConnectionId == Request.Cookies["ConnectionId"]);
                if (hostPlayer == null)
                {
                    return Unauthorized();
                }

                game.IsRoundActive = true;
                _context.SaveChanges();

                // Notify clients to start the round
                _hubContext.Clients.Group(gameLink).SendAsync("RoundStarted");
            }

            return RedirectToAction("GameLobby", new { gameLink = gameLink });
        }

        [HttpPost]
        public IActionResult ResetVotes(string gameLink)
        {
            var game = _context.Games.FirstOrDefault(g => g.GameLink == gameLink);
            if (game != null)
            {
                // Verify that the requester is the host
                var hostPlayer = game.Players.FirstOrDefault(p => p.IsHost && p.ConnectionId == Request.Cookies["ConnectionId"]);
                if (hostPlayer == null)
                {
                    return Unauthorized();
                }

                _context.Votes.RemoveRange(game.Votes);
                game.IsRoundActive = false;
                _context.SaveChanges();

                // Notify clients to reset votes
                _hubContext.Clients.Group(gameLink).SendAsync("VotesReset");
            }

            return RedirectToAction("GameLobby", new { gameLink = gameLink });
        }

        private string GenerateGameLink()
        {
            return System.Guid.NewGuid().ToString("N"); // Long unique string for security
        }
    }
}
