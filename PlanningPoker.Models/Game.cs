namespace PlanningPoker.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GameLink { get; set; }
        public bool HostIsVoter { get; set; } = false;
        public bool IsRoundActive { get; set; }
        public string? RoundName { get; set; }

        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}
