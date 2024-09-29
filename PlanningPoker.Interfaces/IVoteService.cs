using PlanningPoker.Models;

namespace PlanningPoker.Interfaces
{
    public interface IVoteService
    {
        Task SubmitVoteAsync(string gameLink, string cardValue, string connectionId);
        Task<List<Vote>> GetVotesInGameAsync(string gameLink);
        Task ResetVotesAsync(string gameLink);
        Task<bool> HasPlayerVotedAsync(string gameLink, int playerId);
    }
}
