using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TodoApp.Components
{
    public class TodoListViewComponent : ViewComponent
    {
        private readonly TodoContext _context;

        public TodoListViewComponent(TodoContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string statusFilter = "", string sortOrder = "asc", string searchQuery = "")
        {
            var query = _context.TodoItems.AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = statusFilter.ToLower() switch
                {
                    "completed" => query.Where(t => t.IsCompleted),
                    "pending" => query.Where(t => !t.IsCompleted),
                    _ => query
                };
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(t =>
                    (t.Title != null && t.Title.Contains(searchQuery)) ||
                    (t.Description != null && t.Description.Contains(searchQuery)));
            }

            query = sortOrder.ToLower() switch
            {
                "desc" => query.OrderByDescending(t => t.CreatedAt),
                _ => query.OrderBy(t => t.CreatedAt)
            };

            var items = await query.ToListAsync();
            return View(items);
        }
    }
}