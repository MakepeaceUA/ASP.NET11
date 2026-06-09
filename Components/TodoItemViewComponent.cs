using Microsoft.AspNetCore.Mvc;
using TodoApp.Models;

namespace TodoApp.Components
{
    public class TodoItemViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(TodoItem item)
        {
            return View(item);
        }
    }
}