using System.ComponentModel.DataAnnotations;

namespace BloomingTec.Todo.Domain;

public class TaskItem
{
    public Guid Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public bool IsCompleted { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Método de validación que usa el validador de dominio
    public ValidationResult[] Validate()
    {
        return Validators.TaskItemValidator.Validate(this);
    }
}
