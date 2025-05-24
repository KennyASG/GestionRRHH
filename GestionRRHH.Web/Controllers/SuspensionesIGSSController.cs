using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GestionRRHH.Web.Services;

namespace GestionRRHH.Web.Controllers;

[Authorize]
public class SuspensionesIGSSController : Controller
{
    private readonly INominaService _nominaService;

    public SuspensionesIGSSController(INominaService nominaService)
    {
        _nominaService = nominaService;
    }

    // GET: SuspensionesIGSS
    public async Task<IActionResult> Index(DateOnly? fechaInicio, DateOnly? fechaFin, int? empleadoId, string? estado)
    {
        try
        {
            // Si no se especifican fechas, usar el año actual
            if (!fechaInicio.HasValue || !fechaFin.HasValue)
            {
                var hoy = DateOnly.FromDateTime(DateTime.Now);
                fechaInicio = new DateOnly(hoy.Year, 1, 1);
                fechaFin = new DateOnly(hoy.Year, 12, 31);
            }

            var suspensiones = await _nominaService.ObtenerSuspensionesIGSS(fechaInicio, fechaFin, empleadoId, estado);
            var empleados = await _nominaService.ObtenerEmpleadosActivos();

            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;
            ViewBag.EmpleadoId = empleadoId;
            ViewBag.Estado = estado;
            ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto", empleadoId);

            return View(suspensiones);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar suspensiones IGSS: {ex.Message}";
            return View(new List<GestionRRHH.Web.Models.Entities.SuspensionesIgss>());
        }
    }

    // GET: SuspensionesIGSS/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var suspension = await _nominaService.ObtenerSuspensionIGSSPorId(id);
            if (suspension == null)
            {
                TempData["Error"] = "Suspensión IGSS no encontrada.";
                return RedirectToAction("Index");
            }

            return View(suspension);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar detalles: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // GET: SuspensionesIGSS/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        try
        {
            var empleados = await _nominaService.ObtenerEmpleadosActivos();
            ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
            
            return View();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar formulario: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // POST: SuspensionesIGSS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, string tipoSuspension, string? observaciones)
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

            if (fechaInicio == default || fechaFin == default)
            {
                TempData["Error"] = "Las fechas son obligatorias.";
                var empleados = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
                return View();
            }

            if (fechaInicio > fechaFin)
            {
                TempData["Error"] = "La fecha de fin debe ser igual o posterior a la fecha de inicio.";
                var empleados = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
                return View();
            }

            if (string.IsNullOrEmpty(tipoSuspension))
            {
                TempData["Error"] = "Debe seleccionar el tipo de suspensión.";
                var empleados = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
                return View();
            }

            // Verificar que no haya solapamiento con otras suspensiones activas
            var suspensionesExistentes = await _nominaService.ObtenerSuspensionesIGSS(fechaInicio.AddDays(-30), fechaFin.AddDays(30), empleadoId, "activa");
            var hayConflicto = suspensionesExistentes.Any(s => 
                (fechaInicio >= s.FechaInicio && fechaInicio <= s.FechaFin) ||
                (fechaFin >= s.FechaInicio && fechaFin <= s.FechaFin) ||
                (fechaInicio <= s.FechaInicio && fechaFin >= s.FechaFin));

            if (hayConflicto)
            {
                TempData["Error"] = "Ya existe una suspensión IGSS activa para este empleado en el período especificado.";
                var empleados = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
                return View();
            }

            var resultado = await _nominaService.CrearSuspensionIGSS(empleadoId, fechaInicio, fechaFin, tipoSuspension, observaciones);

            if (resultado)
            {
                TempData["Success"] = "Suspensión IGSS registrada exitosamente. El subsidio ha sido calculado automáticamente.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Error al registrar la suspensión IGSS.";
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

    // GET: SuspensionesIGSS/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var suspension = await _nominaService.ObtenerSuspensionIGSSPorId(id);
            if (suspension == null)
            {
                TempData["Error"] = "Suspensión IGSS no encontrada.";
                return RedirectToAction("Index");
            }

            return View(suspension);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar suspensión: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // POST: SuspensionesIGSS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, int diasPagadosIGSS, decimal montoSubsidio, string? observaciones)
    {
        try
        {
            if (diasPagadosIGSS < 0)
            {
                TempData["Error"] = "Los días pagados por IGSS no pueden ser negativos.";
                var suspension = await _nominaService.ObtenerSuspensionIGSSPorId(id);
                return View(suspension);
            }

            if (montoSubsidio < 0)
            {
                TempData["Error"] = "El monto del subsidio no puede ser negativo.";
                var suspension = await _nominaService.ObtenerSuspensionIGSSPorId(id);
                return View(suspension);
            }

            var resultado = await _nominaService.ActualizarSuspensionIGSS(id, diasPagadosIGSS, montoSubsidio, observaciones);

            if (resultado)
            {
                TempData["Success"] = "Suspensión IGSS actualizada exitosamente.";
                return RedirectToAction("Details", new { id });
            }
            else
            {
                TempData["Error"] = "Error al actualizar la suspensión IGSS.";
                var suspension = await _nominaService.ObtenerSuspensionIGSSPorId(id);
                return View(suspension);
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            var suspension = await _nominaService.ObtenerSuspensionIGSSPorId(id);
            return View(suspension);
        }
    }

    // POST: SuspensionesIGSS/Finalizar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizar(int id)
    {
        try
        {
            var resultado = await _nominaService.FinalizarSuspensionIGSS(id);

            if (resultado)
            {
                TempData["Success"] = "Suspensión IGSS finalizada exitosamente.";
            }
            else
            {
                TempData["Error"] = "Error al finalizar la suspensión IGSS.";
            }

            return RedirectToAction("Details", new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Details", new { id });
        }
    }

    // GET: SuspensionesIGSS/Activas
    public async Task<IActionResult> Activas()
    {
        try
        {
            var suspensionesActivas = await _nominaService.ObtenerSuspensionesIGSS(null, null, null, "activa");
            return View(suspensionesActivas);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar suspensiones activas: {ex.Message}";
            return View(new List<GestionRRHH.Web.Models.Entities.SuspensionesIgss>());
        }
    }

    // GET: SuspensionesIGSS/Reporte
    [HttpGet]
    public async Task<IActionResult> Reporte(DateOnly? fechaInicio, DateOnly? fechaFin, int? empleadoId, string? tipoSuspension)
    {
        try
        {
            if (!fechaInicio.HasValue || !fechaFin.HasValue)
            {
                var hoy = DateOnly.FromDateTime(DateTime.Now);
                fechaInicio = new DateOnly(hoy.Year, 1, 1);
                fechaFin = new DateOnly(hoy.Year, 12, 31);
            }

            var suspensiones = await _nominaService.ObtenerSuspensionesIGSS(fechaInicio, fechaFin, empleadoId, null);
            
            if (!string.IsNullOrEmpty(tipoSuspension))
            {
                suspensiones = suspensiones.Where(s => s.Tipo == tipoSuspension).ToList();
            }

            var empleados = await _nominaService.ObtenerEmpleadosActivos();

            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;
            ViewBag.EmpleadoId = empleadoId;
            ViewBag.TipoSuspension = tipoSuspension;
            ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto", empleadoId);

            return View(suspensiones);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al generar reporte: {ex.Message}";
            return RedirectToAction("Index");
        }
    }
}