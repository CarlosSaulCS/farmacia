# Farmacia Desktop

Sistema de escritorio WPF (.NET 8) para la gestión integral de una farmacia: pacientes, consultas, inventario, ventas y reportes.

## Proyectos

- `Farmacia.UI.Wpf`: interfaz WPF basada en MVVM.
- `Farmacia.Domain`: entidades, enums y validaciones.
- `Farmacia.Data`: contexto EF Core y configuraciones.
- `docs/`: documentación funcional y técnica.
- `scripts/`: utilidades para automatizar tareas comunes.

## Requisitos de desarrollo

- Windows 10/11.
- SDK de .NET 8.
- Visual Studio 2022 o VS Code con extensiones C#/XAML (opcional).

## Ejecución en modo desarrollo

```powershell
dotnet restore
dotnet build .
dotnet run --project Farmacia.UI.Wpf/Farmacia.UI.Wpf.csproj
```

## Publicación sin licencias adicionales

Para distribuir la aplicación sin instalar el runtime en cada máquina:

```powershell
pwsh ./scripts/publish.ps1 -Runtime win-x64 -Configuration Release -Package
```

Esto genera una carpeta lista para copiar en `artifacts/publish/<RID>` y un `.zip` comprimido. Consulta `docs/deployment.md` para pasos detallados de despliegue en otra PC.

## Estructura de datos

El sistema usa SQLite; la base se crea automáticamente en la carpeta de ejecución. Para compartir una base pre-poblada, distribuye una copia del archivo `.db` junto con el ejecutable.

## Soporte y mantenimiento

- Ejecuciones manuales de prueba: `dotnet build .`.
- El registro de pacientes, programación de citas y reportes incluyen mensajes de estado para confirmar acciones.
- Ajustes futuros: actualizar scripts y documentación en `docs/` conforme a nuevos módulos o integraciones.
