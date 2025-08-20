
# BloomingTec Todo API

API REST para gestión de tareas desarrollada con .NET 8 y Clean Architecture.

**Estado del proyecto**: ✅ Completamente funcional con Docker y GitHub Actions configurados

## 🐳 Docker Hub

### **Imagen Disponible**

La aplicación está disponible como imagen Docker en Docker Hub:

```bash
# Pull de la imagen oficial
docker pull fernandoguzman/prueba-tecnica-blooming-tec:latest

# O versión específica
docker pull fernandoguzman/prueba-tecnica-blooming-tec:main
```

**Repositorio**: [fernandoguzman/prueba-tecnica-blooming-tec](https://hub.docker.com/r/fernandoguzman/prueba-tecnica-blooming-tec)

### **Ejecución Rápida con Docker**

```bash
# Comando mínimo para ejecutar la API
docker run -d \
  --name todo-api \
  -p 5000:8080 \
  -e BASIC_USER=admin \
  -e BASIC_PASS=password \
  fernandoguzman/prueba-tecnica-blooming-tec:latest

# Verificar que esté funcionando
curl -u admin:password http://localhost:5000/tasks
```

### **Variables de Entorno Configurables**

```bash
docker run -d \
  --name todo-api \
  -p 5000:8080 \
  -e BASIC_USER=tu_usuario \
  -e BASIC_PASS=tu_password \
  -e ASPNETCORE_ENVIRONMENT=Production \
  fernandoguzman/prueba-tecnica-blooming-tec:latest
```

## 📋 Dependencias Mínimas

### **Para Ejecutar con Docker (Recomendado)**

- **Docker Desktop** o **Docker Engine** (versión 20.10+)
- **2GB RAM** mínimo
- **1GB espacio** en disco

### **Para Desarrollo Local**

- **.NET 8.0 SDK** (versión 8.0.0 o superior)
- **SQLite** (incluido con .NET)
- **4GB RAM** recomendado
- **2GB espacio** en disco

### **Para Producción**

- **.NET 8.0 Runtime** (versión 8.0.0 o superior)
- **SQLite** o **SQL Server** (configurable)
- **2GB RAM** mínimo
- **1GB espacio** en disco

## 🚀 Inicio Rápido

### **Opción 1: Docker (Más Simple)**

```bash
# 1. Descargar la imagen
docker pull fernandoguzman/prueba-tecnica-blooming-tec:latest

# 2. Ejecutar la API
docker run -d --name todo-api -p 5000:8080 \
  -e BASIC_USER=admin -e BASIC_PASS=password \
  fernandoguzman/prueba-tecnica-blooming-tec:latest

# 3. Probar la API
curl -u admin:password http://localhost:5000/tasks
```

### **Opción 2: Desarrollo Local**

```bash
# 1. Clonar el repositorio
git clone https://github.com/fernando-guzman-dotnet/prueba-tecnica-blooming-tec.git
cd prueba-tecnica-blooming-tec

# 2. Restaurar dependencias
dotnet restore

# 3. Ejecutar la aplicación
dotnet run --project src/BloomingTec.Todo.Api

# 4. Probar la API
curl -u admin:password http://localhost:5000/tasks
```

### **Opción 3: Docker Compose**

```bash
# 1. Clonar el repositorio
git clone https://github.com/fernando-guzman-dotnet/prueba-tecnica-blooming-tec.git
cd prueba-tecnica-blooming-tec

# 2. Ejecutar con Docker Compose
docker-compose -f docker-compose.dev.yml up --build

# 3. Probar la API
curl -u admin:password http://localhost:5000/tasks
```

## 🔐 Credenciales por Defecto

- **Usuario**: `admin`
- **Contraseña**: `password`

**⚠️ IMPORTANTE**: Cambia estas credenciales en producción usando variables de entorno.

## 📱 Acceso a la API

- **URL Base**: `http://localhost:5000`
- **Swagger UI**: `http://localhost:5000/swagger` (requiere autenticación)
- **Health Check**: `http://localhost:5000/health`

## 🐳 Comandos Docker Útiles

```bash
# Ver logs de la aplicación
docker logs todo-api

# Detener la aplicación
docker stop todo-api

# Reiniciar la aplicación
docker restart todo-api

# Eliminar el contenedor
docker rm todo-api

# Ver estadísticas del contenedor
docker stats todo-api
```

## 🔧 Tecnologías

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


