# Política de Seguridad

## Versiones soportadas

| Versión | Estado |
| --- | --- |
| `main` | Soportada |
| `develop` | Soportada (pre-producción) |
| Otras ramas | Sin soporte |

## Reportar una vulnerabilidad

> ⚠️ **Importante:** si encuentras una vulnerabilidad de seguridad, **no abras un issue público**.

1. Reporta el hallazgo mediante **GitHub Private Vulnerability Reporting**:
   `https://github.com/heartcheck505/HC-Backend/security/advisories/new`
2. Incluye: descripción del problema, pasos para reproducir, impacto esperado y versión/commit afectado.
3. Espera la confirmación de los mantenedores antes de divulgar detalles públicamente.

**Tiempos de respuesta esperados:**

- Confirmación de recepción: 72 horas.
- Respuesta inicial (plan de acción): 7 días máximo.

**Proceso de remediación:**

- Vulnerabilidad **Critical/High**: fix y despliegue lo antes posible (máximo 7 días).
- Vulnerabilidad **Moderate/Low**: fix planificado en el siguiente ciclo de release.

## Buenas prácticas del repositorio

- **Sin secretos en el código**: tokens, claves, connection strings y configuraciones sensibles se inyectan vía variables de entorno, GitHub Secrets o GitHub Actions secrets. Los archivos `appsettings.Production.json`, `appsettings.*.local.json` y `.env` están excluidos por `.gitignore` y nunca deben subirse.
- **Pipeline DevSecOps**: `.github/workflows/devsecops.yml` ejecuta build+test, escaneo de dependencias (gate Critical/High), secret scanning con TruffleHog, análisis estático con CodeQL y escaneo de imagen con Trivy.
- **Dependencias**: Dependabot mantiene NuGet y GitHub Actions actualizados; las vulnerabilidades se resuelven antes de fusionar.
- **JWT**: la `Jwt:SecretKey` de producción debe ser única, aleatoria y de al menos 32 caracteres; la aplicación aborta el arranque con la clave de ejemplo o claves débiles.

## Actualizaciones de seguridad

Las correcciones de seguridad se describen en los releases y en el historial del repositorio siguiendo la convención de mensajes de commit del proyecto.