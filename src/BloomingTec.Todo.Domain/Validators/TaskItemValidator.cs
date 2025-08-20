using System.ComponentModel.DataAnnotations;

namespace BloomingTec.Todo.Domain.Validators;

public static class TaskItemValidator
{
    public static ValidationResult[] Validate(TaskItem task)
    {
        var errors = new List<ValidationResult>();

        // Regla de dominio: Título es obligatorio
        if (string.IsNullOrWhiteSpace(task.Title))
        {
            errors.Add(new ValidationResult("El título es obligatorio", new[] { nameof(task.Title) }));
        }

        // Regla de dominio: Título no puede exceder 100 caracteres
        if (!string.IsNullOrWhiteSpace(task.Title) && task.Title.Length > 100)
        {
            errors.Add(new ValidationResult("El título no debe exceder 100 caracteres", new[] { nameof(task.Title) }));
        }

        // Regla de dominio: Título debe tener al menos 1 carácter
        if (!string.IsNullOrWhiteSpace(task.Title) && task.Title.Length < 1)
        {
            errors.Add(new ValidationResult("El título debe tener al menos 1 carácter", new[] { nameof(task.Title) }));
        }

        // Regla de dominio: Descripción no puede exceder 500 caracteres
        if (!string.IsNullOrWhiteSpace(task.Description) && task.Description.Length > 500)
        {
            errors.Add(new ValidationResult("La descripción no debe exceder 500 caracteres", new[] { nameof(task.Description) }));
        }

        // Regla de dominio: DueDate es obligatorio para tareas importantes
        // Solo validar que no sea nula, sin restricciones de fecha
        if (!task.DueDate.HasValue)
        {
            errors.Add(new ValidationResult("La fecha de vencimiento es obligatoria", new[] { nameof(task.DueDate) }));
        }

        return errors.ToArray();
    }
}
