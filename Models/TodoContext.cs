using Microsoft.EntityFrameworkCore;

namespace TodoApp.Models
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options) : base(options)
        {
        }

        public DbSet<TodoItem> TodoItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TodoItem>().HasData(
                new TodoItem { Id = 1, Title = "Задача 01", Description = "Описание 01", IsCompleted = false, CreatedAt = DateTime.Now.AddDays(-2) },
                new TodoItem { Id = 2, Title = "Задача 02", Description = "Описание 02", IsCompleted = true, CreatedAt = DateTime.Now.AddDays(-1) },
                new TodoItem { Id = 3, Title = "Задача 03", Description = "Описание 03", IsCompleted = false, CreatedAt = DateTime.Now }
            );
        }
    }
}