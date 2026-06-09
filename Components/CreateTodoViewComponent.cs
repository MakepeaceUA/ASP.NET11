using Microsoft.AspNetCore.Mvc;
using TodoApp.Models;

namespace TodoApp.Components
{
    public class CreateTodoViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View(new TodoItem());
        }
    }
}