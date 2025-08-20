using BloomingTec.Todo.Domain;
using Microsoft.EntityFrameworkCore;

namespace BloomingTec.Todo.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Guid.Parse(v));
            entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsCompleted).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Índices para consultas comunes
            entity.HasIndex(e => e.IsCompleted);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Title);
        });

        // Seed data
        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Title = "Completar documentación de la API",
                Description = "Escribir documentación completa de la API con ejemplos",
                IsCompleted = false,
                DueDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Title = "Implementar pruebas unitarias",
                Description = "Agregar pruebas unitarias para todos los métodos del servicio",
                IsCompleted = true,
                DueDate = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            }
        );
    }
}
