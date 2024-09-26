namespace PlanningPoker.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public bool IsHost { get; set; }

        public int GameId { get; set; }
        public virtual Game Game { get; set; }
    }
}
