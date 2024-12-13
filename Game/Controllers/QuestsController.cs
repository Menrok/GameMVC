using Game.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Game.Controllers
{
    public class QuestsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }// GET: Missions/Start/5
         // GET: Quests/Start/5
        public IActionResult Start(int id)
        {
            var quest = _context.Quests.FirstOrDefault(q => q.Id == id);
            if (quest == null)
            {
                return NotFound();
            }

            // Przykład logiki rozpoczęcia misji
            quest.IsCompleted = true; // Zmieniamy status na wykonany (dla testów)
            _context.SaveChanges();

            TempData["Message"] = $"Misja '{quest.Name}' została rozpoczęta!";
            return RedirectToAction("Index", "Quests");
        }


    }
}
