using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GestionRRHH.Web.Services;

namespace GestionRRHH.Web.Controllers;

[Authorize]
public class AsistenciasController : Controller
{
    private readonly INominaService _nominaService;

    public AsistenciasController(INominaService nominaService)
    {
        _nominaService = nominaService;
    }

    // GET: Asistencias
    public async Task<IActionResult> Index(DateOnly? fechaInicio, DateOnly? fechaFin, int? empleadoId)
    {
        try
        {
            // Si no se especifican fechas, usar el mes actual
            if (!fechaInicio.HasValue || !fechaFin.HasValue)
            {
                var hoy = DateOnly.FromDateTime(DateTime.Now);
                fechaInicio = new DateOnly(hoy.Year, hoy.Month, 1);
                fechaFin = fechaInicio.Value.AddMonths(1).AddDays(-1);
            }

            var asistencias = await _nominaService.ObtenerAsistencias(fechaInicio, fechaFin, empleadoId);
            var empleados = await _nominaService.ObtenerEmpleadosActivos();

            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;
            ViewBag.EmpleadoId = empleadoId;
            ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto", empleadoId);

            return View(asistencias);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar asistencias: {ex.Message}";
            return View(new List<GestionRRHH.Web.Models.Entities.Asistencia>());
        }
    }

    // GET: Asistencias/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        try
        {
            var empleados = await _nominaService.ObtenerEmpleadosActivos();
            ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
            
            // Establecer fecha de hoy por defecto
            ViewBag.FechaHoy = DateOnly.FromDateTime(DateTime.Now);
            
            return View();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar formulario: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // POST: Asistencias/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int empleadoId, DateOnly fecha, TimeOnly horaEntrada, TimeOnly? horaSalida, string? observaciones)
    {
        try
        {
            if (empleadoId <= 0)
            {
                TempData["Error"] = "Debe seleccionar un empleado.";
                var empleados = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
                return View();
            }

            if (fecha == default)
            {
                TempData["Error"] = "La fecha es obligatoria.";
                var empleados = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
                return View();
            }

            if (fecha > DateOnly.FromDateTime(DateTime.Now))
            {
                TempData["Error"] = "No se puede registrar asistencia para fechas futuras.";
                var empleados = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
                return View();
            }

            if (horaSalida.HasValue && horaSalida <= horaEntrada)
            {
                TempData["Error"] = "La hora de salida debe ser posterior a la hora de entrada.";
                var empleados = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
                return View();
            }

            var resultado = await _nominaService.RegistrarAsistencia(empleadoId, fecha, horaEntrada, horaSalida, observaciones);

            if (resultado)
            {
                TempData["Success"] = "Asistencia registrada exitosamente.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Error al registrar la asistencia.";
                var empleados = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
                return View();
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            var empleados = await _nominaService.ObtenerEmpleadosActivos();
            ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
            return View();
        }
    }

    // GET: Asistencias/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var asistencia = await _nominaService.ObtenerAsistenciaPorId(id);
            if (asistencia == null)
            {
                TempData["Error"] = "Asistencia no encontrada.";
                return RedirectToAction("Index");
            }

            var empleados = await _nominaService.ObtenerEmpleadosActivos();
            ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto", asistencia.IdEmpleado);

            return View(asistencia);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar asistencia: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // POST: Asistencias/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, int empleadoId, DateOnly fecha, TimeOnly horaEntrada, TimeOnly? horaSalida, string? observaciones)
    {
        try
        {
            if (horaSalida.HasValue && horaSalida <= horaEntrada)
            {
                TempData["Error"] = "La hora de salida debe ser posterior a la hora de entrada.";
                var asistenciaError = await _nominaService.ObtenerAsistenciaPorId(id);
                var empleadosError = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleadosError, "Id", "NombreCompleto", empleadoId);
                return View(asistenciaError);
            }

            var resultado = await _nominaService.RegistrarAsistencia(empleadoId, fecha, horaEntrada, horaSalida, observaciones);

            if (resultado)
            {
                TempData["Success"] = "Asistencia actualizada exitosamente.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Error al actualizar la asistencia.";
                var asistencia = await _nominaService.ObtenerAsistenciaPorId(id);
                var empleados = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto", empleadoId);
                return View(asistencia);
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            var asistencia = await _nominaService.ObtenerAsistenciaPorId(id);
            var empleados = await _nominaService.ObtenerEmpleadosActivos();
            ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto", empleadoId);
            return View(asistencia);
        }
    }

    // POST: Asistencias/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var resultado = await _nominaService.EliminarAsistencia(id);

            if (resultado)
            {
                TempData["Success"] = "Asistencia eliminada exitosamente.";
            }
            else
            {
                TempData["Error"] = "Error al eliminar la asistencia.";
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index");
        }
    }

    // GET: Asistencias/Reporte
    [HttpGet]
    public async Task<IActionResult> Reporte(DateOnly? fechaInicio, DateOnly? fechaFin, int? empleadoId)
    {
        try
        {
            if (!fechaInicio.HasValue || !fechaFin.HasValue)
            {
                var hoy = DateOnly.FromDateTime(DateTime.Now);
                fechaInicio = new DateOnly(hoy.Year, hoy.Month, 1);
                fechaFin = fechaInicio.Value.AddMonths(1).AddDays(-1);
            }

            // Usar el nuevo método del servicio
            var reporteData = await _nominaService.ObtenerReporteAsistencias(fechaInicio, fechaFin, empleadoId);
            var empleados = await _nominaService.ObtenerEmpleadosActivos();

            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;
            ViewBag.EmpleadoId = empleadoId;
            ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto", empleadoId);
            ViewBag.ReporteData = reporteData; // <- NUEVA LÍNEA: Pasar los datos del reporte

            // Seguimos enviando las asistencias como modelo para mantener compatibilidad
            var asistencias = ((dynamic)reporteData).Asistencias as List<GestionRRHH.Web.Models.Entities.Asistencia>;
            return View(asistencias);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al generar reporte: {ex.Message}";
            return RedirectToAction("Index");
        }
    }
}