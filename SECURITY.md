# Política de Seguridad

## Versiones Soportadas

| Versión | Soportada          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |

## Reportar una Vulnerabilidad

Agradecemos que reportes vulnerabilidades de seguridad. Por favor, **NO** abras un issue público para reportar una vulnerabilidad de seguridad.

### Proceso de Reporte

1. **Email de Seguridad**: Envía un email a [security@bloomingtec.com](mailto:security@bloomingtec.com)
2. **Título del Email**: Usa el formato `[SECURITY] Descripción breve de la vulnerabilidad`
3. **Contenido**: Incluye:
   - Descripción detallada de la vulnerabilidad
   - Pasos para reproducir
   - Impacto potencial
   - Sugerencias de mitigación (si las tienes)

### Respuesta

- **Acknowledgment**: Recibirás confirmación en 48 horas
- **Investigation**: Investigaremos la vulnerabilidad reportada
- **Update**: Te mantendremos informado del progreso
- **Fix**: Desarrollaremos y probaremos una solución
- **Disclosure**: Coordinaremos la divulgación pública

### Timeline

- **48 horas**: Confirmación inicial
- **7 días**: Evaluación inicial
- **30 días**: Plan de remediación
- **90 días**: Implementación de la solución

## Mejores Prácticas de Seguridad

### Para Desarrolladores

- **Dependencias**: Mantén las dependencias actualizadas
- **Secretos**: Nunca commits credenciales o secretos
- **Validación**: Valida siempre la entrada del usuario
- **Autenticación**: Implementa autenticación robusta
- **Autorización**: Verifica permisos en cada endpoint

### Para Usuarios

- **Credenciales**: Cambia las credenciales por defecto
- **HTTPS**: Usa siempre conexiones seguras
- **Actualizaciones**: Mantén la aplicación actualizada
- **Monitoreo**: Monitorea logs de acceso

## Configuración de Seguridad

### Variables de Entorno

```bash
# Cambia estas credenciales por defecto
BASIC_USER=tu_usuario_seguro
BASIC_PASS=tu_password_seguro

# Para producción, usa passwords fuertes
BASIC_PASS=TuPasswordFuerte123!@#
```

### Headers de Seguridad

La API incluye headers de seguridad:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`

### Autenticación

- **Basic Auth**: Implementado en todos los endpoints
- **Rate Limiting**: Considerado para futuras versiones
- **JWT**: Considerado para futuras versiones

## Auditoría de Seguridad

### Herramientas Utilizadas

- **Trivy**: Escaneo de vulnerabilidades en Docker
- **CodeQL**: Análisis estático de código
- **OWASP ZAP**: Testing de seguridad automatizado

### Escaneos Regulares

- **Dependencias**: Semanalmente con Dependabot
- **Contenedores**: En cada build de Docker
- **Código**: En cada Pull Request

## Historial de Vulnerabilidades

### 2024-08-19
- **Vulnerabilidad**: Credenciales por defecto
- **Estado**: ✅ Resuelto
- **Solución**: Variables de entorno configurables

## Contacto

- **Email de Seguridad**: [security@bloomingtec.com](mailto:security@bloomingtec.com)
- **PGP Key**: Disponible bajo solicitud
- **Responsable**: Equipo de Seguridad de BloomingTec

## Agradecimientos

Agradecemos a todos los investigadores de seguridad que reportan vulnerabilidades de manera responsable y nos ayudan a mantener la seguridad de nuestra aplicación.
