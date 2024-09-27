namespace PlanningPoker.Models
{
    public class Vote
    {
        public int Id { get; set; }
        public string Card { get; set; }

        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }

        public int GameId { get; set; }
        public virtual Game Game { get; set; }
    }
}
