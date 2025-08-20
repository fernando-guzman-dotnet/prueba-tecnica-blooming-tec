
# BloomingTec Todo API

API REST para gestión de tareas desarrollada con .NET 8 y Clean Architecture.

**Estado del proyecto**: ✅ Completamente funcional con Docker y GitHub Actions configurados

## Tecnologías

- **Lenguaje:** C# (.NET 8)
- **Base de datos:** SQLite con Entity Framework Core
- **Autenticación:** Basic Auth (configurable por variables de entorno)
- **Documentación:** Swagger / OpenAPI
- **Pruebas:** xUnit con WebApplicationFactory
- **Containerización:** Docker

## Arquitectura

- **Domain:** Entidades y reglas de negocio
- **Application:** Casos de uso y lógica
- **Infrastructure:** Persistencia y servicios externos
- **API:** Endpoints HTTP (Minimal APIs)

## Endpoints

| Método | Endpoint     | Descripción                |
|--------|-------------|----------------------------|
| GET    | /tasks      | Listar tareas con filtros  |
| GET    | /tasks/{id} | Obtener tarea por ID       |
| POST   | /tasks      | Crear nueva tarea          |
| PUT    | /tasks/{id} | Actualizar tarea existente |
| DELETE | /tasks/{id} | Eliminar tarea             |

### Parámetros de consulta (`GET /tasks`)
- `search`: búsqueda por título
- `isCompleted`: filtrar por estado (true/false)
- `createdFrom`, `createdTo`: rango de fechas
- `sortBy`: `createdAt`, `title`, `dueDate`, `isCompleted`
- `sortDesc`: orden descendente (true por defecto)

### Validaciones
- Título obligatorio (≤100 caracteres, único para tareas activas)
- Descripción ≤500 caracteres
- Fecha de vencimiento obligatoria
- Título corto requiere descripción

## Autenticación

- **Basic Auth en todos los endpoints**
- Credenciales por defecto: `admin:password`
- Variables de entorno:
  ```bash
  BASIC_USER=admin
  BASIC_PASS=password

## Manejo de Errores

* `400 Bad Request`: parámetros inválidos, validaciones fallidas
* `401 Unauthorized`: credenciales ausentes/incorrectas
* `404 Not Found`: recurso no encontrado
* `500 Internal Server Error`: errores inesperados

## Documentación

* **Swagger UI:** [http://localhost:5000/swagger](http://localhost:5000/swagger)
* **OpenAPI JSON:** [http://localhost:5000/swagger/v1/swagger.json](http://localhost:5000/swagger/v1/swagger.json)
* Soporta autenticación con botón *Authorize*

## Ejemplos con cURL

### Crear Tarea

```bash
curl -X POST "http://localhost:5000/tasks" \
  -H "Authorization: Basic YWRtaW46cGFzc3dvcmQ=" \
  -H "Content-Type: application/json" \
  -d '{"title":"Completar documentación","description":"Escribir README","dueDate":"2025-12-31T23:59:59Z"}'
```

### Listar Tareas

```bash
curl -X GET "http://localhost:5000/tasks?search=documentación&sortBy=createdAt" \
  -H "Authorization: Basic YWRtaW46cGFzc3dvcmQ="
```

### Actualizar Tarea

```bash
curl -X PUT "http://localhost:5000/tasks/{id}" \
  -H "Authorization: Basic YWRtaW46cGFzc3dvcmQ=" \
  -H "Content-Type: application/json" \
  -d '{"title":"Título actualizado","isCompleted":true}'
```

### Eliminar Tarea

```bash
curl -X DELETE "http://localhost:5000/tasks/{id}" \
  -H "Authorization: Basic YWRtaW46cGFzc3dvcmQ="
```

## Ejecución y Despliegue

### Local

```bash
dotnet restore
dotnet run --project src/BloomingTec.Todo.Api
```

### Docker

```bash
docker build -t bloomingtec-todo-api .
docker run -p 5000:8080 -e BASIC_USER=admin -e BASIC_PASS=password bloomingtec-todo-api
```

### Acceso

* API: [http://localhost:5000](http://localhost:5000)
* Swagger: [http://localhost:5000/swagger](http://localhost:5000/swagger)

## Pruebas

```bash
dotnet test
```

Cobertura: autenticación, CRUD, validaciones, manejo de errores y base de datos en memoria.

## Estructura del Proyecto

```
src/
├── BloomingTec.Todo.Api/           # Presentación (Minimal APIs)
├── BloomingTec.Todo.Application/   # Casos de uso
├── BloomingTec.Todo.Domain/        # Entidades de negocio
└── BloomingTec.Todo.Infrastructure/# Persistencia

tests/
└── BloomingTec.Todo.Api.Tests/     # Pruebas de integración
```

## Licencia

© BloomingTec 2025


