using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GestionRRHH.Web.Services;

namespace GestionRRHH.Web.Controllers;

[Authorize]
public class VacacionesController : Controller
{
    private readonly INominaService _nominaService;

    public VacacionesController(INominaService nominaService)
    {
        _nominaService = nominaService;
    }

    // GET: Vacaciones
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

            var vacaciones = await _nominaService.ObtenerVacaciones(fechaInicio, fechaFin, empleadoId, estado);
            var empleados = await _nominaService.ObtenerEmpleadosActivos();

            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;
            ViewBag.EmpleadoId = empleadoId;
            ViewBag.Estado = estado;
            ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto", empleadoId);

            return View(vacaciones);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar vacaciones: {ex.Message}";
            return View(new List<GestionRRHH.Web.Models.Entities.Vacacione>());
        }
    }

    // GET: Vacaciones/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var vacacion = await _nominaService.ObtenerVacacionesPorId(id);
            if (vacacion == null)
            {
                TempData["Error"] = "Vacación no encontrada.";
                return RedirectToAction("Index");
            }

            return View(vacacion);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar detalles: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // GET: Vacaciones/Create
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

    // POST: Vacaciones/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, int diasVacaciones)
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

            if (fechaInicio >= fechaFin)
            {
                TempData["Error"] = "La fecha de fin debe ser posterior a la fecha de inicio.";
                var empleados = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
                return View();
            }

            if (diasVacaciones <= 0)
            {
                TempData["Error"] = "Los días de vacaciones deben ser mayor a cero.";
                var empleados = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
                return View();
            }

            // Verificar días disponibles
            var diasDisponibles = await _nominaService.CalcularDiasVacacionesDisponibles(empleadoId);
            if (diasVacaciones > diasDisponibles)
            {
                TempData["Error"] = $"El empleado solo tiene {diasDisponibles} días de vacaciones disponibles.";
                var empleados = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
                return View();
            }

            var resultado = await _nominaService.SolicitarVacaciones(empleadoId, fechaInicio, fechaFin, diasVacaciones);

            if (resultado)
            {
                TempData["Success"] = "Vacaciones solicitadas exitosamente.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Error al solicitar las vacaciones.";
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

    // POST: Vacaciones/Aprobar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(int id)
    {
        try
        {
            var resultado = await _nominaService.AprobarVacaciones(id);

            if (resultado)
            {
                TempData["Success"] = "Vacaciones aprobadas exitosamente.";
            }
            else
            {
                TempData["Error"] = "Error al aprobar las vacaciones.";
            }

            return RedirectToAction("Details", new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Details", new { id });
        }
    }

    // POST: Vacaciones/Rechazar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(int id, string motivo)
    {
        try
        {
            var resultado = await _nominaService.RechazarVacaciones(id, motivo ?? "Sin motivo especificado");

            if (resultado)
            {
                TempData["Success"] = "Vacaciones rechazadas.";
            }
            else
            {
                TempData["Error"] = "Error al rechazar las vacaciones.";
            }

            return RedirectToAction("Details", new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Details", new { id });
        }
    }

    // GET: Vacaciones/ConsultarDisponibles/5
    [HttpGet]
    public async Task<IActionResult> ConsultarDisponibles(int empleadoId)
    {
        try
        {
            var diasDisponibles = await _nominaService.CalcularDiasVacacionesDisponibles(empleadoId);
            return Json(new { success = true, dias = diasDisponibles });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }
}