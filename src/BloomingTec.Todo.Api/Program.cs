using BloomingTec.Todo.Application;
using BloomingTec.Todo.Domain;
using BloomingTec.Todo.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// Servicios
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "BloomingTec API de Tareas", 
        Version = "v1",
        Description = "API REST para gestionar tareas con autenticación básica"
    });
    
            c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "basic",
            Description = "Encabezado de autenticación básica"
        });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "basic" }
            },
            new string[] {}
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Problem Details
builder.Services.AddProblemDetails();

// No se necesita configuración adicional para validación manual

var app = builder.Build();

// Habilitar CORS
app.UseCors();

// Auto-aplicar migraciones de base de datos en el arranque (según proveedor)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        if (db.Database.IsRelational())
        {
            db.Database.Migrate();
            // Seed mínimo si está vacío
            if (!db.Tasks.Any())
            {
                db.Tasks.AddRange(
                    new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        Title = "Completar documentación de la API",
                        Description = "Escribir documentación completa de la API con ejemplos",
                        IsCompleted = false,
                        DueDate = DateTime.UtcNow.AddDays(7),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        Title = "Implementar pruebas unitarias",
                        Description = "Agregar pruebas unitarias para todos los métodos del servicio",
                        IsCompleted = true,
                        DueDate = DateTime.UtcNow.AddDays(-1),
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        UpdatedAt = DateTime.UtcNow.AddDays(-1)
                    }
                );
                db.SaveChanges();
            }
        }
        else
        {
            db.Database.EnsureCreated();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al aplicar migraciones en el arranque");
        throw;
    }
}

// Middleware de Autenticación Básica (antes de Swagger para proteger la documentación)
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    
    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
    {
        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"BloomingTec API de Tareas\"";
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("No autorizado");
        return;
    }

    try
    {
        var encodedCredentials = authHeader.Substring("Basic ".Length);
        var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
        var separatorIndex = credentials.IndexOf(':');
        
        if (separatorIndex < 0)
        {
            throw new ArgumentException("Invalid credentials format");
        }

        var username = credentials.Substring(0, separatorIndex);
        var password = credentials.Substring(separatorIndex + 1);

        var expectedUser = context.RequestServices.GetRequiredService<IConfiguration>()["BASIC_USER"] ?? "admin";
        var expectedPass = context.RequestServices.GetRequiredService<IConfiguration>()["BASIC_PASS"] ?? "password";

        bool isValid = username == expectedUser && password == expectedPass;

        if (!isValid)
        {
            context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"BloomingTec API de Tareas\"";
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("No autorizado");
            return;
        }
    }
    catch
    {
        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"BloomingTec API de Tareas\"";
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("No autorizado");
        return;
    }

    await next();
});

// Configurar el pipeline HTTP
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BloomingTec API de Tareas v1");
    c.RoutePrefix = "swagger";
});

// Quitar redirección HTTPS en desarrollo
// app.UseHttpsRedirection();

// Endpoints de la API
app.MapGet("/tasks", async (
    ITaskService taskService,
    string? search = null,
    bool? isCompleted = null,
    DateTime? createdFrom = null,
    DateTime? createdTo = null,
    string sortBy = "createdAt",
    bool sortDesc = true) =>
{
    try
    {
        var allowedSort = new[]{"createdAt","title","dueDate","isCompleted"};
        if (!allowedSort.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            return Results.ValidationProblem(new Dictionary<string, string[]>{{"sortBy", new[]{"sortBy no permitido"}}}, statusCode:400, title:"Datos inválidos");
        if (createdFrom.HasValue && createdTo.HasValue && createdFrom > createdTo)
            return Results.ValidationProblem(new Dictionary<string, string[]>{{"dateRange", new[]{"createdFrom debe ser <= createdTo"}}}, statusCode:400, title:"Datos inválidos");

        var tasks = await taskService.GetAllTasksAsync(search, isCompleted, createdFrom, createdTo, sortBy, sortDesc);
        return Results.Ok(tasks);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Error al obtener tareas",
            detail: ex.Message,
            statusCode: 500,
            type: "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        );
    }
})
.WithName("GetTasks")
.WithOpenApi(operation => new(operation)
{
    Summary = "Obtiene la lista de tareas",
    Description = "Filtrado y orden: search, isCompleted, createdFrom/createdTo, sortBy (createdAt|title|dueDate|isCompleted), sortDesc",
    Parameters = new List<Microsoft.OpenApi.Models.OpenApiParameter>
    {
        new()
        {
            Name = "search",
            In = Microsoft.OpenApi.Models.ParameterLocation.Query,
            Description = "Filtro por título (contiene, sin distinción de mayúsculas)",
            Required = false,
            Schema = new() { Type = "string" }
        },
        new(){ Name = "isCompleted", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Description = "Filtra por estado de completado", Required = false, Schema = new(){ Type = "boolean" }},
        new(){ Name = "createdFrom", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Description = "Fecha de creación desde (UTC)", Required = false, Schema = new(){ Type = "string", Format = "date-time" }},
        new(){ Name = "createdTo", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Description = "Fecha de creación hasta (UTC)", Required = false, Schema = new(){ Type = "string", Format = "date-time" }},
        new(){ Name = "sortBy", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Description = "Campo de orden: createdAt|title|dueDate|isCompleted", Required = false, Schema = new(){ Type = "string", Default = new Microsoft.OpenApi.Any.OpenApiString("createdAt") }},
        new(){ Name = "sortDesc", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Description = "Orden descendente (true por defecto)", Required = false, Schema = new(){ Type = "boolean", Default = new Microsoft.OpenApi.Any.OpenApiBoolean(true) }}
    }
});

app.MapGet("/tasks/{id}", async (Guid id, ITaskService taskService) =>
{
    try
    {
        var task = await taskService.GetTaskByIdAsync(id);
        return Results.Ok(task);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.Problem(
            title: "Tarea no encontrada",
            detail: ex.Message,
            statusCode: 404,
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.4"
        );
    }
    catch (ArgumentException ex)
    {
        return Results.Problem(
            title: "ID inválido",
            detail: ex.Message,
            statusCode: 400,
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        );
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Error al obtener tarea",
            detail: ex.Message,
            statusCode: 500,
            type: "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        );
    }
})
.WithName("GetTask")
.WithOpenApi(operation => new(operation)
{
    Summary = "Obtiene una tarea individual por ID",
    Description = "Devuelve una tarea cuando se proporciona un ID válido",
    Parameters = new List<Microsoft.OpenApi.Models.OpenApiParameter>
    {
        new()
        {
            Name = "id",
            In = Microsoft.OpenApi.Models.ParameterLocation.Path,
            Description = "Identificador de la tarea",
            Required = true,
            Schema = new() { Type = "string", Format = "uuid" }
        }
    }
});

app.MapPost("/tasks", async (TaskItem task, ITaskService taskService) =>
{
    try
    {
        var createdTask = await taskService.CreateTaskAsync(task);
        return Results.CreatedAtRoute("GetTask", new { id = createdTask.Id }, createdTask);
    }
    catch (ValidationException ex)
    {
        // Convertir ValidationException a ValidationProblem
        return Results.ValidationProblem(
            new Dictionary<string, string[]> { { "general", new[] { ex.Message } } },
            statusCode: 400,
            title: "Datos inválidos",
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        );
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Error inesperado al crear tarea",
            detail: ex.Message,
            statusCode: 500,
            type: "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        );
    }
})
.WithName("CreateTask")
.Accepts<TaskItem>("application/json")
.Produces<TaskItem>(StatusCodes.Status201Created)
.ProducesValidationProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status500InternalServerError)
.WithOpenApi(operation =>
{
    operation.Summary = "Crea una nueva tarea";
    operation.Description = "Crea una nueva tarea con las propiedades proporcionadas";
    operation.RequestBody ??= new Microsoft.OpenApi.Models.OpenApiRequestBody();
    operation.RequestBody.Description = "Carga útil de la tarea con título, descripción, fecha de vencimiento y estado de completado";
    operation.RequestBody.Required = true;
    // Ejemplo por defecto con isCompleted = false
    operation.RequestBody.Content ??= new Dictionary<string, Microsoft.OpenApi.Models.OpenApiMediaType>();
    operation.RequestBody.Content["application/json"] = new Microsoft.OpenApi.Models.OpenApiMediaType
    {
        Example = new OpenApiObject
        {
            ["id"] = new OpenApiString(Guid.NewGuid().ToString()),
            ["title"] = new OpenApiString("Tarea de ejemplo"),
            ["description"] = new OpenApiString("Descripción opcional"),
            ["isCompleted"] = new OpenApiBoolean(false),
            ["dueDate"] = new OpenApiString(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")),
            ["createdAt"] = new OpenApiString(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")),
            ["updatedAt"] = new OpenApiString(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))
        }
    };
    return operation;
});

app.MapPut("/tasks/{id}", async (Guid id, TaskItem taskUpdate, ITaskService taskService) =>
{
    try
    {
        var updatedTask = await taskService.UpdateTaskAsync(id, taskUpdate);
        return Results.Ok(updatedTask);
    }
    catch (ArgumentNullException ex)
    {
        return Results.Problem(
            title: "Datos de actualización inválidos",
            detail: ex.Message,
            statusCode: 400,
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        );
    }
    catch (KeyNotFoundException ex)
    {
        return Results.Problem(
            title: "Tarea no encontrada",
            detail: ex.Message,
            statusCode: 404,
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.4"
        );
    }
    catch (ValidationException ex)
    {
        // Convertir ValidationException a ValidationProblem
        return Results.ValidationProblem(
            new Dictionary<string, string[]> { { "general", new[] { ex.Message } } },
            statusCode: 400,
            title: "Datos inválidos",
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        );
    }
    catch (ArgumentException ex)
    {
        return Results.Problem(
            title: "ID inválido",
            detail: ex.Message,
            statusCode: 400,
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        );
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Error inesperado al actualizar tarea",
            detail: ex.Message,
            statusCode: 500,
            type: "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        );
    }
})
.WithName("UpdateTask")
.Accepts<TaskItem>("application/json")
.Produces<TaskItem>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status404NotFound)
.ProducesValidationProblem(StatusCodes.Status400BadRequest)
.WithOpenApi(operation =>
{
    operation.Summary = "Actualiza una tarea existente";
    operation.Description = "Actualiza la tarea identificada por ID con las propiedades proporcionadas";
    operation.Parameters = new List<Microsoft.OpenApi.Models.OpenApiParameter>
    {
        new()
        {
            Name = "id",
            In = Microsoft.OpenApi.Models.ParameterLocation.Path,
            Description = "Identificador de la tarea a actualizar",
            Required = true,
            Schema = new() { Type = "string", Format = "uuid" }
        }
    };
    operation.RequestBody ??= new Microsoft.OpenApi.Models.OpenApiRequestBody();
    operation.RequestBody.Description = "Carga útil de la tarea actualizada";
    operation.RequestBody.Required = true;
    return operation;
});

app.MapDelete("/tasks/{id}", async (Guid id, ITaskService taskService) =>
{
    try
    {
        await taskService.DeleteTaskAsync(id);
        return Results.NoContent();
    }
    catch (KeyNotFoundException ex)
    {
        return Results.Problem(
            title: "Tarea no encontrada",
            detail: ex.Message,
            statusCode: 404,
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.4"
        );
    }
    catch (ValidationException ex)
    {
        return Results.Problem(
            title: "No se puede eliminar la tarea",
            detail: ex.Message,
            statusCode: 400,
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        );
    }
    catch (ArgumentException ex)
    {
        return Results.Problem(
            title: "ID inválido",
            detail: ex.Message,
            statusCode: 400,
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        );
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Error al eliminar tarea",
            detail: ex.Message,
            statusCode: 500,
            type: "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        );
    }
})
.WithName("DeleteTask")
.WithOpenApi(operation => new(operation)
{
    Summary = "Elimina una tarea",
    Description = "Elimina la tarea identificada por ID",
    Parameters = new List<Microsoft.OpenApi.Models.OpenApiParameter>
    {
        new()
        {
            Name = "id",
            In = Microsoft.OpenApi.Models.ParameterLocation.Path,
            Description = "Identificador de la tarea a eliminar",
            Required = true,
            Schema = new() { Type = "integer", Minimum = 1 }
        }
    }
});

// Health endpoint removed

app.Run();

// Make Program class public for testing
public partial class Program { }
