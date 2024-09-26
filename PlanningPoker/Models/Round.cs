namespace PlanningPoker.Models
{
    public class Round
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

        public int GameId { get; set; }
        public virtual Game Game { get; set; }

        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}
