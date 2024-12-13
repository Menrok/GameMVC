using Game.Data;
using Game.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Game.Controllers
{
    public class GameController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GameController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GameDashboard(int heroId)
        {
            var hero = await _context.Heroes
                .FirstOrDefaultAsync(h => h.Id == heroId);

            if (hero == null)
            {
                return NotFound();
            }

            ViewData["HeroId"] = hero.Id;

            return View(hero);
        }

        [HttpGet]
        public async Task<IActionResult> Missions(int heroId)
        {
            var hero = await _context.Heroes
                .FirstOrDefaultAsync(h => h.Id == heroId);

            if (hero == null)
            {
                return NotFound();
            }

            var quests = await _context.Quests.ToListAsync();
            var heroQuests = await _context.HeroQuests
                .Where(hq => hq.HeroId == heroId)
                .ToListAsync();

            ViewBag.Quests = quests;
            ViewBag.HeroQuests = heroQuests;
            ViewData["HeroId"] = hero.Id;

            return View(hero);
        }

        [HttpGet]
        public async Task<IActionResult> Hero(int heroId)
        {
            var hero = await _context.Heroes
                .Include(h => h.Items)
                .FirstOrDefaultAsync(h => h.Id == heroId);

            if (hero == null)
            {
                return NotFound();
            }

            ViewData["HeroId"] = hero.Id;

            return View(hero);
        }

        [HttpGet]
        public async Task<IActionResult> Shop(int heroId)
        {
            var hero = await _context.Heroes
                .Include(h => h.Items)
                .FirstOrDefaultAsync(h => h.Id == heroId);

            if (hero == null)
            {
                return NotFound();
            }

            var weapons = await _context.Items.Where(i => i.Type == "Broń").ToListAsync();
            var armors = await _context.Items.Where(i => i.Type == "Zbroja").ToListAsync();
            var amulets = await _context.Items.Where(i => i.Type == "Amulet").ToListAsync();

            ViewBag.Weapons = weapons ?? new List<Game.Models.Item>();
            ViewBag.Armors = armors ?? new List<Game.Models.Item>();
            ViewBag.Amulets = amulets ?? new List<Game.Models.Item>();
            ViewBag.Inventory = hero.Items ?? new List<Game.Models.Item>();

            ViewData["HeroId"] = hero.Id;

            return View(hero);
        }

        [HttpPost]
        public async Task<IActionResult> PurchaseItem(int heroId, int itemId)
        {
            var hero = await _context.Heroes.Include(h => h.Items)
                                            .FirstOrDefaultAsync(h => h.Id == heroId);
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId);

            if (hero == null || item == null || hero.Money < item.Price)
            {
                TempData["ErrorMessage"] = "Nie masz wystarczająco złota.";
                return RedirectToAction("Shop", new { heroId });
            }

            if (hero.Items.Contains(item))
            {
                TempData["ErrorMessage"] = "Już posiadasz ten przedmiot.";
                return RedirectToAction("Shop", new { heroId });
            }

            hero.Money -= item.Price;
            hero.Items.Add(item);

            await _context.SaveChangesAsync();
            return RedirectToAction("Shop", new { heroId });
        }

        [HttpPost]
        public async Task<IActionResult> SellItem(int heroId, int itemId)
        {
            var hero = await _context.Heroes
                                      .Include(h => h.Items) 
                                      .FirstOrDefaultAsync(h => h.Id == heroId);

            var item = await _context.Items
                                      .FirstOrDefaultAsync(i => i.Id == itemId);

            if (hero == null || item == null)
            {
                return NotFound();
            }

            if (hero.Items.Contains(item))
            {
                hero.Money += item.Price;

                hero.Items.Remove(item);

                _context.Items.Remove(item);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Shop", new { heroId });
        }

        [HttpPost]
        public async Task<IActionResult> EquipItem(int heroId, int itemId)
        {
            var hero = await _context.Heroes
                                     .Include(h => h.Items)
                                     .FirstOrDefaultAsync(h => h.Id == heroId);

            var item = await _context.Items
                                      .FirstOrDefaultAsync(i => i.Id == itemId);

            if (hero == null || item == null)
            {
                return NotFound();
            }

            if (!item.IsEquipped)
            {
                var existingItem = hero.Items
                                       .FirstOrDefault(i => i.Type == item.Type && i.IsEquipped);

                if (existingItem != null)
                {
                    existingItem.IsEquipped = false;

                    hero.Strength -= existingItem.BonusStrength;
                    hero.Dexterity -= existingItem.BonusDexterity;
                    hero.Intelligence -= existingItem.BonusIntelligence;
                    hero.Defense -= existingItem.BonusDefense;
                    hero.Health -= existingItem.BonusHealth;
                }

                hero.Strength += item.BonusStrength;
                hero.Dexterity += item.BonusDexterity;
                hero.Intelligence += item.BonusIntelligence;
                hero.Defense += item.BonusDefense;
                hero.Health += item.BonusHealth;

                item.IsEquipped = true;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Hero", new { heroId });
        }

        [HttpPost]
        public async Task<IActionResult> UnequipItem(int heroId, int itemId)
        {
            var hero = await _context.Heroes
                                     .Include(h => h.Items)
                                     .FirstOrDefaultAsync(h => h.Id == heroId);
            var item = await _context.Items
                                      .FirstOrDefaultAsync(i => i.Id == itemId);

            if (hero == null || item == null)
            {
                return NotFound();
            }

            if (item.IsEquipped)
            {
                hero.Strength -= item.BonusStrength;
                hero.Dexterity -= item.BonusDexterity;
                hero.Intelligence -= item.BonusIntelligence;
                hero.Defense -= item.BonusDefense;
                hero.Health -= item.BonusHealth;

                item.IsEquipped = false;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Hero", new { heroId });
        }

        [HttpGet]
        public async Task<IActionResult> Fight(int heroId)
        {
            if (heroId == 0)
            {
                return NotFound("Bohater nie został znaleziony.");
            }

            var hero = await _context.Heroes
                .Include(h => h.Items)
                .FirstOrDefaultAsync(h => h.Id == heroId);

            if (hero == null)
            {
                return NotFound("Bohater nie został znaleziony.");
            }

            var enemies = await _context.Enemies.ToListAsync();

            if (enemies == null || !enemies.Any())
            {
                return NotFound("Brak przeciwników w bazie danych.");
            }

            var random = new Random();
            var enemy = enemies[random.Next(enemies.Count)];

            if (enemy == null)
            {
                return NotFound("Przeciwnik nie został znaleziony.");
            }

            ViewData["HeroId"] = hero.Id;
            ViewData["EnemyId"] = enemy.Id;

            return View(new FightViewModel
            {
                Hero = hero,
                Enemy = enemy
            });
        }

        [HttpPost]
        public async Task<IActionResult> Fight(FightViewModel model)
        {
            var hero = await _context.Heroes
                .FirstOrDefaultAsync(h => h.Id == model.HeroId);

            var enemy = await _context.Enemies
                .FirstOrDefaultAsync(e => e.Id == model.EnemyId);

            if (hero == null || enemy == null)
            {
                return NotFound();
            }

            var heroPower = hero.Strength + hero.Dexterity + hero.Intelligence;
            var enemyPower = enemy.Strength + enemy.Dexterity + enemy.Intelligence;

            var heroWins = heroPower > enemyPower;

            if (heroWins)
            {
                hero.Money += 50;
                hero.Experience += 100;

                TempData["FightResult"] = "Wygrałeś walkę!";
                TempData["GoldReward"] = 50;
                TempData["ExperienceReward"] = 100;
            }
            else
            {
                TempData["FightResult"] = "Przegrałeś walkę. Spróbuj ponownie!";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("FightResult", new { heroId = hero.Id });
        }


        [HttpGet]
        public async Task<IActionResult> FightResult(int heroId)
        {
            var hero = await _context.Heroes
                .FirstOrDefaultAsync(h => h.Id == heroId);

            if (hero == null)
            {
                return NotFound("Bohater nie został znaleziony.");
            }

            var enemy = await _context.Enemies
                .FirstOrDefaultAsync(e => e.Id == hero.Id); 

            if (enemy == null)
            {
                return NotFound("Przeciwnik nie został znaleziony.");
            }

            return View(new FightViewModel
            {
                Hero = hero,
                Enemy = enemy
            });
        }
    }
}