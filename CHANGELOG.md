# Changelog

Todos los cambios notables en este proyecto serán documentados en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
y este proyecto adhiere a [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- GitHub Actions para CI/CD
- Análisis de seguridad con CodeQL
- Escaneo de vulnerabilidades con Trivy
- Plantillas para Issues y Pull Requests
- Guía de contribución
- Política de seguridad

### Changed
- Optimización del Dockerfile
- Mejoras en docker-compose
- Configuración de Dependabot

## [1.0.0] - 2024-08-19

### Added
- API REST completa para gestión de tareas
- Operaciones CRUD (Create, Read, Update, Delete)
- Autenticación básica en todos los endpoints
- Validaciones de dominio robustas
- Manejo de errores estándar RFC 7807
- Documentación interactiva con Swagger
- Base de datos SQLite con Entity Framework Core
- Arquitectura limpia (Clean Architecture)
- Pruebas unitarias con xUnit
- Containerización con Docker
- Middleware de autenticación personalizado
- Validaciones de negocio
- Filtros y ordenamiento en consultas
- Migraciones automáticas de base de datos

### Technical Details
- .NET 8 con ASP.NET Core Minimal APIs
- Entity Framework Core 8
- SQLite para desarrollo y testing
- xUnit para testing
- Docker multi-stage build
- GitHub Actions para CI/CD

### Security Features
- Autenticación básica configurable
- Validación de entrada robusta
- Headers de seguridad
- Manejo seguro de errores

### Documentation
- README completo en español
- Documentación de API con Swagger
- Ejemplos de uso con cURL
- Guía de despliegue
- Estructura del proyecto

## [0.1.0] - 2024-08-18

### Added
- Estructura inicial del proyecto
- Configuración básica de .NET 8
- Proyectos de arquitectura limpia
- Configuración inicial de Entity Framework
- Middleware básico de autenticación

---

## Notas de Versión

### Versionado Semántico
- **MAJOR**: Cambios incompatibles con versiones anteriores
- **MINOR**: Nueva funcionalidad compatible
- **PATCH**: Correcciones de bugs compatibles

### Criterios de Release
- **Alpha**: Funcionalidad básica implementada
- **Beta**: Funcionalidad completa, testing en progreso
- **RC**: Release candidate, testing final
- **Stable**: Versión de producción

### Soporte
- **Versión Actual**: 1.0.0
- **Versión LTS**: 1.0.x (soporte extendido)
- **Versiones Anteriores**: No soportadas

---

## Contribuir al Changelog

Para contribuir al changelog:
1. Agrega entradas en la sección `[Unreleased]`
2. Usa el formato estándar de Keep a Changelog
3. Incluye todos los cambios relevantes
4. Mantén el formato consistente

## Enlaces

- [Keep a Changelog](https://keepachangelog.com/)
- [Semantic Versioning](https://semver.org/)
- [Conventional Commits](https://www.conventionalcommits.org/)
