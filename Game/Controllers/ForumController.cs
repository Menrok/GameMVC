using Game.Data;
using Game.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

namespace Game.Controllers
{
    public class ForumController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ForumController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var threads = _context.ForumThreads
                .Include(t => t.User)
                .ToList();
            return View(threads);
        }

        public IActionResult Show(int id)
        {
            var thread = _context.ForumThreads
                .Include(t => t.User)
                .FirstOrDefault(t => t.Id == id);

            if (thread == null)
            {
                return NotFound();
            }

            var replies = _context.Replies
                .Where(r => r.ThreadId == id)
                .Include(r => r.User)
                .ToList();

            var viewModel = new ThreadRepliesViewModel
            {
                Thread = thread,
                Replies = replies
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult CreateThread()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateThread(ForumThread thread)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var userId = _context.Users.FirstOrDefault(u => u.Email == email)?.Id;

                if (userId != null)
                {
                    thread.UserId = userId.Value;
                    thread.CreatedAt = DateTime.Now;

                    _context.ForumThreads.Add(thread);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Nie udało się znaleźć użytkownika.");
            }

            return View(thread);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddReply(int threadId, string content)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(content))
            {
                ModelState.AddModelError("", "Treść odpowiedzi nie może być pusta.");
                return RedirectToAction("Show", new { id = threadId });
            }

            var userId = _context.Users.FirstOrDefault(u => u.Email == email)?.Id;

            if (userId != null)
            {
                var reply = new Reply
                {
                    ThreadId = threadId,
                    Content = content,
                    UserId = userId.Value,
                    CreatedAt = DateTime.Now
                };

                _context.Replies.Add(reply);
                _context.SaveChanges();

                return RedirectToAction("Show", new { id = threadId });
            }

            ModelState.AddModelError("", "Nie udało się pobrać identyfikatora użytkownika.");
            return RedirectToAction("Show", new { id = threadId });
        }
    }
}