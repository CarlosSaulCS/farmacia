# Despliegue local de Farmacia

Este documento describe cómo generar un paquete auto-contenido y desplegar la aplicación WPF en otra computadora sin adquirir licencias adicionales.

## 1. Requisitos en la máquina de empaquetado

- Windows 10/11 con PowerShell 5+ o PowerShell 7.
- SDK de .NET 8 instalado (ya presente en el entorno de desarrollo).
- Acceso al código fuente de este repositorio.

## 2. Generar la publicación

Desde la raíz del repositorio, ejecuta el script de publicación:

```powershell
pwsh ./scripts/publish.ps1 -Runtime win-x64 -Configuration Release -Package
```

Parámetros disponibles:

- `-Runtime`: RID objetivo (`win-x64`, `win-x86`, `win-arm64`). Por defecto `win-x64`.
- `-Configuration`: `Release` o `Debug`. Por defecto `Release`.
- `-SingleFile`: agrega `/p:PublishSingleFile=true` para generar un ejecutable único (útil al distribuir pocos archivos).
- `-Package`: genera un archivo `.zip` dentro de `artifacts\publish` listo para copiar a otra máquina.

El script invoca `dotnet publish` con `--self-contained true`, por lo que el runtime de .NET 8 queda incluido en el paquete y no es necesario instalar nada adicional en la computadora destino.

## 3. Contenido del paquete

Tras ejecutarse el script:

- `artifacts/publish/<RID>/` contendrá los binarios listos para usar.
- `artifacts/Farmacia-<RID>-<Configuration>-<timestamp>.zip` (si usaste `-Package`) contendrá la misma carpeta comprimida.

Archivos clave:

- `Farmacia.UI.Wpf.exe`: ejecutable principal.
- `Farmacia.db` (si se distribuye una base de datos inicial). Actualmente la solución usa SQLite; si no existe el archivo, se crea automáticamente en la carpeta de ejecución.
- `appsettings.json` u otros archivos de configuración (si se agregan en el futuro).

## 4. Instalación en otra PC

1. Copia el `.zip` generado a la nueva máquina.
2. Descomprime el contenido en `C:\Program Files\Farmacia` (o la ruta preferida del usuario). No se requieren privilegios de administrador si usas una carpeta del usuario.
3. Crea un acceso directo a `Farmacia.UI.Wpf.exe` en el escritorio o menú inicio (opcional).
4. Ejecuta la aplicación; los módulos de Pacientes, Consultorio, Inventario y Reportes funcionarán con la base local. No se necesita conexión a internet.

## 5. Consideraciones de base de datos

- El proyecto usa SQLite. Si deseas compartir datos comunes, distribuye un archivo `Farmacia.db` inicial dentro del paquete. Cada instalación puede apuntar a su propia copia local para trabajo independiente.
- La utilidad `Farmacia.Tools` incluida en la solución facilita revisar el estado de la base y automatizar respaldos/restauraciones locales:

	```powershell
	# Inicializa o vuelve a generar datos de ejemplo antes de operar
	dotnet run --project .\Farmacia.Tools\Farmacia.Tools.csproj -- init

	# Listar tablas, conteos e integridad
	dotnet run --project .\Farmacia.Tools\Farmacia.Tools.csproj -- inspect

	# Crear un respaldo en la carpeta indicada
	dotnet run --project .\Farmacia.Tools\Farmacia.Tools.csproj -- backup .\backups

	# Restaurar desde un respaldo previo
	dotnet run --project .\Farmacia.Tools\Farmacia.Tools.csproj -- restore .\backups\farmacia-backup-YYYYMMDDHHMMSS.db
	```

	La herramienta usa el servicio `DatabaseMaintenanceService`, de modo que los mismos pasos pueden exponerse dentro de la aplicación si se requiere en el futuro.
- Para respaldos manuales también basta con copiar el archivo `.db`. Incluye procedimientos de respaldo/restauración en el manual de usuario si es necesario.
- Existe un flujo automatizado en GitHub Actions (`.github/workflows/nightly-backup.yml`) que ejecuta diariamente el comando de respaldo y publica el archivo generado como artefacto con retención de siete días. Puedes dispararlo manualmente con *Run workflow* desde la pestaña **Actions** cuando necesites un respaldo inmediato.

## 6. Actualizaciones futuras

Para entregar una nueva versión:

1. Ejecuta nuevamente `publish.ps1` (idealmente con `-Package`).
2. Distribuye el nuevo `.zip`. Los usuarios pueden sobrescribir la carpeta existente conservando respaldos de la base de datos.

## 7. Problemas frecuentes

| Problema | Solución |
|----------|----------|
| Windows SmartScreen bloquea la app | Como el ejecutable no está firmado, SmartScreen puede advertir la primera vez. Indica "Más información" → "Ejecutar de todas formas". |
| Falta de permisos al colocar en `Program Files` | Usa una carpeta dentro de `%LOCALAPPDATA%` o ejecuta el copiado con privilegios elevados. |
| La app no inicia y falta la BD | Verifica que la carpeta tenga permisos de escritura. La aplicación necesita crear/modificar el archivo SQLite. |

Con estos pasos puedes desplegar la aplicación en ambientes locales sin comprar licencias ni instalar herramientas adicionales.
