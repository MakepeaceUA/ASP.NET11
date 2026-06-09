using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Localization;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    public class TaskController : Controller
    {
        private readonly TodoContext _context;
        private readonly IStringLocalizer<TaskController> _localizer;
        private readonly IMemoryCache _cache;

        public TaskController(TodoContext context, IStringLocalizer<TaskController> localizer, IMemoryCache cache)
        {
            _context = context;
            _localizer = localizer;
            _cache = cache;
        }

        [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "statusFilter", "sortOrder", "searchQuery" })]
        public IActionResult Index(string statusFilter, string sortOrder, string searchQuery)
        {
            ViewData["StatusFilter"] = statusFilter;
            ViewData["SortOrder"] = sortOrder;
            ViewData["SearchQuery"] = searchQuery;

            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
            string cacheKey = $"task_details_{id}";
            if (!_cache.TryGetValue(cacheKey, out TodoItem? selectedTask))
            {
                selectedTask = await _context.TodoItems.FindAsync(id);
                if (selectedTask == null)
                {
                    return NotFound();
                }
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _cache.Set(cacheKey, selectedTask, cacheOptions);
            }
            return View(selectedTask);
        }

        public IActionResult ChangeLanguage(string culture, string returnUrl = "/")
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TodoItem newTask)
        {
            if (string.IsNullOrWhiteSpace(newTask.Title))
            {
                ModelState.AddModelError("Title", _localizer["TitleEmptyError"]);
            }

            if (ModelState.IsValid)
            {
                newTask.CreatedAt = DateTime.Now;
                newTask.IsCompleted = false;
                _context.TodoItems.Add(newTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(newTask);
        }

        [HttpPost]
        public async Task<IActionResult> Complete(int id)
        {
            var taskToComplete = await _context.TodoItems.FindAsync(id);
            if (taskToComplete == null)
            {
                return NotFound();
            }
            taskToComplete.IsCompleted = true;
            await _context.SaveChangesAsync();
            
            _cache.Remove($"task_details_{id}");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var taskToDelete = await _context.TodoItems.FindAsync(id);
            if (taskToDelete == null)
            {
                return NotFound();
            }
            _context.TodoItems.Remove(taskToDelete);
            await _context.SaveChangesAsync();

            _cache.Remove($"task_details_{id}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetTasksJson()
        {
            var allTasks = await _context.TodoItems.ToListAsync();
            return Json(allTasks);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadTasks()
        {
            var tasksToDownload = await _context.TodoItems.ToListAsync();
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("ID,Title,Description,IsCompleted,CreatedAt");
            foreach (var taskItem in tasksToDownload)
            {
                csvBuilder.AppendLine($"{taskItem.Id},\"{taskItem.Title}\",\"{taskItem.Description}\",{taskItem.IsCompleted},{taskItem.CreatedAt:yyyy-MM-dd HH:mm}");
            }

            return File(Encoding.UTF8.GetBytes(csvBuilder.ToString()), "text/csv", "tasks.csv");
        }
    }
}