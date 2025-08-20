using BloomingTec.Todo.Domain;
using BloomingTec.Todo.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BloomingTec.Todo.Application;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskItem>> GetAllTasksAsync(
        string? searchQuery = null,
        bool? isCompleted = null,
        DateTime? createdFrom = null,
        DateTime? createdTo = null,
        string sortBy = "createdAt",
        bool sortDesc = true)
    {
        var query = _context.Tasks.AsQueryable();

        // Filtro por título (case-insensitive)
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var normalized = searchQuery.Trim();
            query = query.Where(t => EF.Functions.Like(EF.Functions.Collate(t.Title, "NOCASE"), $"%{normalized}%"));
        }

        // Filtro por estado
        if (isCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == isCompleted.Value);
        }

        // Rango de fechas de creación
        if (createdFrom.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= createdFrom.Value);
        }
        if (createdTo.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= createdTo.Value);
        }

        // Ordenamiento permitido
        var sortKey = sortBy?.Trim().ToLower() ?? "createdat";
        query = (sortKey, sortDesc) switch
        {
            ("title", false) => query.OrderBy(t => t.Title),
            ("title", true)  => query.OrderByDescending(t => t.Title),
            ("createdat", false) => query.OrderBy(t => t.CreatedAt),
            ("createdat", true)  => query.OrderByDescending(t => t.CreatedAt),
            ("duedate", false) => query.OrderBy(t => t.DueDate),
            ("duedate", true)  => query.OrderByDescending(t => t.DueDate),
            ("iscompleted", false) => query.OrderBy(t => t.IsCompleted),
            ("iscompleted", true)  => query.OrderByDescending(t => t.IsCompleted),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        return await query.ToListAsync();
    }

    public async Task<TaskItem> GetTaskByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El ID de la tarea no puede estar vacío", nameof(id));
        }

        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            throw new KeyNotFoundException($"No se encontró la tarea con ID {id}");
        }

        return task;
    }

    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        // Validaciones de entrada
        if (task == null)
        {
            throw new ArgumentNullException(nameof(task), "La tarea no puede ser null");
        }

        // Validación de dominio antes de crear
        var validationErrors = task.Validate();
        if (validationErrors.Length > 0)
        {
            var errorMessage = string.Join("; ", validationErrors.Select(e => e.ErrorMessage));
            throw new ValidationException($"La tarea no cumple con las reglas de validación: {errorMessage}");
        }

        // Asegurar valores por defecto para campos requeridos
        task.IsCompleted = false; // Valor por defecto explícito
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        
        return task;
    }

    public async Task<TaskItem> UpdateTaskAsync(Guid id, TaskItem taskUpdate)
    {
        // Validaciones de entrada
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El ID de la tarea no puede estar vacío", nameof(id));
        }

        if (taskUpdate == null)
        {
            throw new ArgumentNullException(nameof(taskUpdate), "Los datos de actualización no pueden ser null");
        }

        // Validación de dominio antes de buscar la tarea
        var validationErrors = taskUpdate.Validate();
        if (validationErrors.Length > 0)
        {
            var errorMessage = string.Join("; ", validationErrors.Select(e => e.ErrorMessage));
            throw new ValidationException($"La tarea no cumple con las reglas de validación: {errorMessage}");
        }

        var existingTask = await _context.Tasks.FindAsync(id);
        if (existingTask == null)
            throw new KeyNotFoundException($"No se encontró la tarea con ID {id}");

        // Actualizar solo los campos permitidos
        existingTask.Title = taskUpdate.Title;
        existingTask.Description = taskUpdate.Description;
        existingTask.IsCompleted = taskUpdate.IsCompleted;
        existingTask.DueDate = taskUpdate.DueDate;
        existingTask.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingTask;
    }

    public async Task DeleteTaskAsync(Guid id)
    {
        // Validaciones de entrada
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El ID de la tarea no puede estar vacío", nameof(id));
        }

        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
            throw new KeyNotFoundException($"No se encontró la tarea con ID {id}");

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }
}
