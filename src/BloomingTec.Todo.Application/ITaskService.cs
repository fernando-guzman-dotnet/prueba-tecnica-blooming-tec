using BloomingTec.Todo.Domain;
using System.ComponentModel.DataAnnotations;

namespace BloomingTec.Todo.Application;

/// <summary>
/// Servicio para la gestión de tareas con validaciones de negocio robustas
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Obtiene todas las tareas con filtros opcionales y ordenamiento
    /// </summary>
    /// <param name="searchQuery">Texto para buscar en el título de las tareas</param>
    /// <param name="isCompleted">Filtrar por estado de completado</param>
    /// <param name="createdFrom">Filtrar tareas creadas desde esta fecha</param>
    /// <param name="createdTo">Filtrar tareas creadas hasta esta fecha</param>
    /// <param name="sortBy">Campo por el cual ordenar (title, createdAt, dueDate, isCompleted)</param>
    /// <param name="sortDesc">Orden descendente si es true, ascendente si es false</param>
    /// <returns>Colección de tareas filtradas y ordenadas</returns>
    Task<IEnumerable<TaskItem>> GetAllTasksAsync(
        string? searchQuery = null,
        bool? isCompleted = null,
        DateTime? createdFrom = null,
        DateTime? createdTo = null,
        string sortBy = "createdAt",
        bool sortDesc = true);

    /// <summary>
    /// Obtiene una tarea específica por su ID
    /// </summary>
    /// <param name="id">ID único de la tarea</param>
    /// <returns>La tarea encontrada</returns>
    /// <exception cref="ArgumentException">Se lanza cuando el ID está vacío</exception>
    /// <exception cref="KeyNotFoundException">Se lanza cuando la tarea no existe</exception>
    Task<TaskItem> GetTaskByIdAsync(Guid id);

    /// <summary>
    /// Crea una nueva tarea con validaciones de dominio y negocio
    /// </summary>
    /// <param name="task">Datos de la tarea a crear</param>
    /// <returns>La tarea creada con ID asignado</returns>
    /// <exception cref="ArgumentNullException">Se lanza cuando la tarea es null</exception>
    /// <exception cref="ValidationException">Se lanza cuando no se cumplen las validaciones</exception>
    Task<TaskItem> CreateTaskAsync(TaskItem task);

    /// <summary>
    /// Actualiza una tarea existente con validaciones de coherencia
    /// </summary>
    /// <param name="id">ID de la tarea a actualizar</param>
    /// <param name="taskUpdate">Datos actualizados de la tarea</param>
    /// <returns>La tarea actualizada</returns>
    /// <exception cref="ArgumentException">Se lanza cuando el ID está vacío</exception>
    /// <exception cref="ArgumentNullException">Se lanza cuando los datos de actualización son null</exception>
    /// <exception cref="KeyNotFoundException">Se lanza cuando la tarea no existe</exception>
    /// <exception cref="ValidationException">Se lanza cuando no se cumplen las validaciones</exception>
    Task<TaskItem> UpdateTaskAsync(Guid id, TaskItem taskUpdate);

    /// <summary>
    /// Elimina una tarea con validaciones de negocio
    /// </summary>
    /// <param name="id">ID de la tarea a eliminar</param>
    /// <returns>Void</returns>
    /// <exception cref="ArgumentException">Se lanza cuando el ID está vacío</exception>
    /// <exception cref="KeyNotFoundException">Se lanza cuando la tarea no existe</exception>
    /// <exception cref="ValidationException">Se lanza cuando no se puede eliminar por reglas de negocio</exception>
    Task DeleteTaskAsync(Guid id);
}
