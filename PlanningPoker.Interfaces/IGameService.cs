using PlanningPoker.Models;

namespace PlanningPoker.Interfaces
{
    public interface IGameService
    {
        Task<Game> CreateGameAsync(string gameName, bool hostIsVoter);
        Task<Game> GetGameByLinkAsync(string gameLink);
        Task StartRoundAsync(string gameLink, string roundName, string connectionId);
        Task EndRoundAsync(string gameLink, string connectionId);
        Task ResetVotesAsync(string gameLink, string connectionId);
    }
}
