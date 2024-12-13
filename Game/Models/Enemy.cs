namespace Game.Models
{
    public class Enemy
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Intelligence { get; set; }
        public int Health { get; set; }
        public int Level { get; set; }
        public int GoldReward { get; set; }
        public int ExperienceReward { get; set; }
    }
}
