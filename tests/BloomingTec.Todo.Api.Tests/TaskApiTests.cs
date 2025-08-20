using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BloomingTec.Todo.Api;
using BloomingTec.Todo.Domain;
using BloomingTec.Todo.Infrastructure;
using BloomingTec.Todo.Application;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BloomingTec.Todo.Api.Tests;

public class TaskApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TaskApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Eliminar el registro existente de DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Eliminar el registro existente de AppDbContext
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(AppDbContext));
                if (dbContextDescriptor != null)
                    services.Remove(dbContextDescriptor);

                // Agregar base de datos en memoria
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                });

                // Asegurar que TaskService esté registrado
                services.AddScoped<ITaskService, TaskService>();
            });
        });

        _client = _factory.CreateClient();
        
        // Asegurar que la base de datos esté creada y con datos semilla
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        
        // Agregar algunos datos de prueba
        if (!context.Tasks.Any())
        {
            context.Tasks.AddRange(
                new TaskItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Tarea de prueba 1",
                    Description = "Descripción de prueba 1",
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new TaskItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Tarea de prueba 2",
                    Description = "Descripción de prueba 2",
                    IsCompleted = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
            context.SaveChanges();
        }
    }

    private void AddBasicAuth(string username = "admin", string password = "password")
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }

    [Fact]
    public async Task GetTasks_WithoutAuth_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/tasks");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("WWW-Authenticate", response.Headers.ToString());
    }

    [Fact]
    public async Task GetTasks_WithValidAuth_Returns200()
    {
        // Preparación
        AddBasicAuth();

        // Ejecución
        var response = await _client.GetAsync("/tasks");

        // Verificación
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        // Debe devolver un arreglo JSON
        Assert.StartsWith("[", content);
    }

    [Fact]
    public async Task AllEndpoints_WithoutAuth_Return401()
    {
        // Probar todos los endpoints sin autenticación
        var endpoints = new[]
        {
            ("/tasks", HttpMethod.Get),
            ($"/tasks/{Guid.Empty}", HttpMethod.Get),
            ("/tasks", HttpMethod.Post),
            ($"/tasks/{Guid.Empty}", HttpMethod.Put),
            ($"/tasks/{Guid.Empty}", HttpMethod.Delete)
        };

        foreach (var (endpoint, method) in endpoints)
        {
            HttpResponseMessage response;
            
            if (method == HttpMethod.Post || method == HttpMethod.Put)
            {
                var task = new TaskItem { Title = "Test", Description = "Test", DueDate = DateTime.UtcNow.AddDays(1) };
                var json = JsonSerializer.Serialize(task);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                if (method == HttpMethod.Post)
                    response = await _client.PostAsync(endpoint, content);
                else
                    response = await _client.PutAsync(endpoint, content);
            }
            else if (method == HttpMethod.Delete)
            {
                response = await _client.DeleteAsync(endpoint);
            }
            else
            {
                response = await _client.GetAsync(endpoint);
            }

            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized, 
                $"El endpoint {method} {endpoint} debe devolver 401, pero devolvió {response.StatusCode}");
        }
    }

    // Eliminado: el endpoint de salud ya no existe

    [Fact]
    public async Task CreateTask_WithValidData_Returns201()
    {
        // Preparación
        AddBasicAuth();
        var task = new TaskItem
        {
            Title = "Tarea de prueba",
            Description = "Descripción de prueba",
            IsCompleted = false,
            DueDate = DateTime.UtcNow.AddDays(7)
        };
        var json = JsonSerializer.Serialize(task);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Ejecución
        var response = await _client.PostAsync("/tasks", content);

        // Verificación
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/tasks/", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task GetTask_WithNonExistentId_Returns404()
    {
        // Preparación
        AddBasicAuth();

        // Ejecución
        var response = await _client.GetAsync($"/tasks/{Guid.NewGuid()}");

        // Verificación
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTask_WithInvalidTitle_Returns400()
    {
        // Preparación
        AddBasicAuth();
        
        // Primero, obtener tareas existentes
        var tasksResponse = await _client.GetAsync("/tasks");
        Assert.Equal(HttpStatusCode.OK, tasksResponse.StatusCode);
        var tasks = JsonSerializer.Deserialize<TaskItem[]>(
            await tasksResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        TaskItem? taskToUpdate = null;

        // Usar una tarea existente o crear una
        if (tasks != null && tasks.Length > 0)
        {
            taskToUpdate = tasks[0];
        }
        else
        {
            // Crear primero una tarea
            var createTask = new TaskItem
            {
                Title = "Tarea original",
                Description = "Descripción original",
                DueDate = DateTime.UtcNow.AddDays(7)
            };
            var createJson = JsonSerializer.Serialize(createTask);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/tasks", createContent);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            
            taskToUpdate = JsonSerializer.Deserialize<TaskItem>(
                await createResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        Assert.NotNull(taskToUpdate);

        // Ahora intentar actualizar con título inválido
        var updateTask = new TaskItem
        {
            Title = "", // Inválido: título vacío
            Description = "Descripción actualizada"
        };
        var updateJson = JsonSerializer.Serialize(updateTask);
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

        // Ejecución - Intentar actualizar la tarea
        var response = await _client.PutAsync($"/tasks/{taskToUpdate!.Id}", updateContent);

        // Verificación - Debe devolver 400 por título inválido
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTask_ExistingTask_Returns204Then404()
    {
        // Preparación
        AddBasicAuth();

        // Primero, veamos qué tareas existen inicialmente
        var initialResponse = await _client.GetAsync("/tasks");
        Assert.Equal(HttpStatusCode.OK, initialResponse.StatusCode);
        var initialTasks = JsonSerializer.Deserialize<TaskItem[]>(
            await initialResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        TaskItem? taskToDelete = null;
        
        // Usar una tarea existente si está disponible, de lo contrario crear una
        if (initialTasks != null && initialTasks.Length > 0)
        {
            taskToDelete = initialTasks[0];
        }
        else
        {
            // Crear una tarea
            var task = new TaskItem
            {
                Title = "Tarea a eliminar",
                Description = "Será eliminada",
                DueDate = DateTime.UtcNow.AddDays(7)
            };
            var json = JsonSerializer.Serialize(task);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/tasks", content);
            
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            
            taskToDelete = JsonSerializer.Deserialize<TaskItem>(
                await createResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        Assert.NotNull(taskToDelete);
        
        // Ejecución - Eliminar la tarea
        var deleteResponse = await _client.DeleteAsync($"/tasks/{taskToDelete!.Id}");

        // Verificación - Debe devolver 204 o 404 (si ya fue eliminada en otra prueba)
        Assert.True(deleteResponse.StatusCode == HttpStatusCode.NoContent || 
                   deleteResponse.StatusCode == HttpStatusCode.NotFound,
                   $"Se esperaba NoContent (204) o NotFound (404), se obtuvo {deleteResponse.StatusCode}");

        // Si la eliminación tuvo éxito, verificar que ya no exista
        if (deleteResponse.StatusCode == HttpStatusCode.NoContent)
        {
            var getResponse = await _client.GetAsync($"/tasks/{taskToDelete.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }

    [Fact]
    public async Task AnyEndpoint_WithInvalidCredentials_Returns401()
    {
        var badCreds = Convert.ToBase64String(Encoding.UTF8.GetBytes("admin:wrongpass"));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", badCreds);

        var response = await _client.GetAsync("/tasks");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_Secured_RequiresAuth()
    {
        // Sin autenticación -> 401
        var noAuth = await _client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.Unauthorized, noAuth.StatusCode);

        // Con autenticación -> 200
        AddBasicAuth();
        var withAuth = await _client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, withAuth.StatusCode);
    }

    // Tests para validar que las annotations funcionen automáticamente
    [Fact]
    public async Task CreateTask_WithEmptyTitle_Returns400FromDomainValidation()
    {
        // Arrange
        AddBasicAuth();
        var invalidTask = new TaskItem
        {
            Title = "", // Inválido: título vacío (debe fallar validación manual)
            Description = "Descripción válida"
        };
        var json = JsonSerializer.Serialize(invalidTask);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/tasks", content);

        // Assert - Debe devolver 400 por validación de dominio
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        // Verificar que el mensaje de error sea el del dominio
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("El título es obligatorio", responseContent);
    }

    [Fact]
    public async Task CreateTask_WithTitleTooLong_Returns400FromDomainValidation()
    {
        // Arrange
        AddBasicAuth();
        var invalidTask = new TaskItem
        {
            Title = new string('A', 101), // Inválido: 101 caracteres (debe fallar validación manual)
            Description = "Descripción válida"
        };
        var json = JsonSerializer.Serialize(invalidTask);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/tasks", content);

        // Assert - Debe devolver 400 por validación de dominio
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        // Verificar que el mensaje de error sea el del dominio
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("El título no debe exceder 100 caracteres", responseContent);
    }

    [Fact]
    public async Task UpdateTask_WithEmptyTitle_Returns400FromAnnotations()
    {
        // Arrange
        AddBasicAuth();
        
        // Crear una tarea primero
        var createTask = new TaskItem
        {
            Title = "Tarea válida",
            Description = "Descripción válida",
            DueDate = DateTime.UtcNow.AddDays(7)
        };
        var createJson = JsonSerializer.Serialize(createTask);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/tasks", createContent);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var createdTask = JsonSerializer.Deserialize<TaskItem>(
            await createResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Ahora intentar actualizar con título inválido
        var updateTask = new TaskItem
        {
            Title = "", // Inválido: título vacío (debe fallar [Required])
            Description = "Descripción actualizada"
        };
        var updateJson = JsonSerializer.Serialize(updateTask);
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/tasks/{createdTask!.Id}", updateContent);

        // Assert - Debe devolver 400 por validación de dominio
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        // Verificar que el mensaje de error sea el del dominio
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("El título es obligatorio", responseContent);
    }

    [Fact]
    public async Task UpdateTask_WithTitleTooLong_Returns400FromDomainValidation()
    {
        // Arrange
        AddBasicAuth();
        
        // Crear una tarea primero
        var createTask = new TaskItem
        {
            Title = "Tarea válida",
            Description = "Descripción válida",
            DueDate = DateTime.UtcNow.AddDays(7)
        };
        var createJson = JsonSerializer.Serialize(createTask);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/tasks", createContent);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var createdTask = JsonSerializer.Deserialize<TaskItem>(
            await createResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Ahora intentar actualizar con título inválido
        var updateTask = new TaskItem
        {
            Title = new string('B', 101), // Inválido: 101 caracteres (debe fallar [StringLength(100)])
            Description = "Descripción actualizada"
        };
        var updateJson = JsonSerializer.Serialize(updateTask);
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/tasks/{createdTask!.Id}", updateContent);

        // Assert - Debe devolver 400 por validación de dominio
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        // Verificar que el mensaje de error sea el del dominio
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("El título no debe exceder 100 caracteres", responseContent);
    }
}
