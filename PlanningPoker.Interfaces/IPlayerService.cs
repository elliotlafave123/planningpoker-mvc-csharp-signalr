using PlanningPoker.Models;

namespace PlanningPoker.Interfaces
{
    public interface IPlayerService
    {
        Task<Player> JoinGameAsync(string gameLink, string playerName, string connectionId);
        Task<Player> GetPlayerByConnectionIdAsync(string connectionId);
        Task RemovePlayerAsync(string connectionId);
        Task<List<Player>> GetPlayersInGameAsync(string gameLink);
    }
}
