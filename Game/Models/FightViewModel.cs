namespace Game.Models
{
    public class FightViewModel
    {
        public Hero Hero { get; set; }
        public Enemy Enemy { get; set; }
        public int HeroId { get; set; }
        public int EnemyId { get; set; }
    }
}
