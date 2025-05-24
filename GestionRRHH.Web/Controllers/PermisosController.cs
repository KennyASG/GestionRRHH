using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GestionRRHH.Web.Services;

namespace GestionRRHH.Web.Controllers;

[Authorize]
public class PermisosController : Controller
{
    private readonly INominaService _nominaService;

    public PermisosController(INominaService nominaService)
    {
        _nominaService = nominaService;
    }

    // GET: Permisos
    public async Task<IActionResult> Index(DateOnly? fechaInicio, DateOnly? fechaFin, int? empleadoId, string? estado)
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

            var permisos = await _nominaService.ObtenerPermisos(fechaInicio, fechaFin, empleadoId, estado);
            var empleados = await _nominaService.ObtenerEmpleadosActivos();

            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;
            ViewBag.EmpleadoId = empleadoId;
            ViewBag.Estado = estado;
            ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto", empleadoId);

            return View(permisos);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar permisos: {ex.Message}";
            return View(new List<GestionRRHH.Web.Models.Entities.Permiso>());
        }
    }

    // GET: Permisos/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var permiso = await _nominaService.ObtenerPermisoPorId(id);
            if (permiso == null)
            {
                TempData["Error"] = "Permiso no encontrado.";
                return RedirectToAction("Index");
            }

            return View(permiso);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar detalles: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // GET: Permisos/Create
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

    // POST: Permisos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, string tipo, string motivo, int dias, decimal? descuento)
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

            if (string.IsNullOrEmpty(tipo))
            {
                TempData["Error"] = "Debe seleccionar el tipo de permiso.";
                var empleados = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
                return View();
            }

            if (string.IsNullOrEmpty(motivo))
            {
                TempData["Error"] = "El motivo es obligatorio.";
                var empleados = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
                return View();
            }

            if (dias <= 0)
            {
                TempData["Error"] = "Los días de permiso deben ser mayor a cero.";
                var empleados = await _nominaService.ObtenerEmpleadosActivos();
                ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
                return View();
            }

            // Si es sin goce de sueldo, calcular descuento básico si no se especifica
            if (tipo == "sin_goce" && !descuento.HasValue)
            {
                // Aquí podrías calcular el descuento basado en el salario del empleado
                descuento = 0; // Por ahora lo dejamos en 0, se puede calcular después
            }

            var resultado = await _nominaService.SolicitarPermiso(empleadoId, fechaInicio, fechaFin, tipo, motivo, dias, descuento);

            if (resultado)
            {
                TempData["Success"] = "Permiso solicitado exitosamente.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Error al solicitar el permiso.";
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

    // POST: Permisos/Aprobar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(int id)
    {
        try
        {
            var resultado = await _nominaService.AprobarPermiso(id);

            if (resultado)
            {
                TempData["Success"] = "Permiso aprobado exitosamente.";
            }
            else
            {
                TempData["Error"] = "Error al aprobar el permiso.";
            }

            return RedirectToAction("Details", new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Details", new { id });
        }
    }

    // POST: Permisos/Rechazar/5
    [HttpPost]
    [ValidateAntiForgeryToken]  
    public async Task<IActionResult> Rechazar(int id)
    {
        try
        {
            var resultado = await _nominaService.RechazarPermiso(id);

            if (resultado)
            {
                TempData["Success"] = "Permiso rechazado.";
            }
            else
            {
                TempData["Error"] = "Error al rechazar el permiso.";
            }

            return RedirectToAction("Details", new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Details", new { id });
        }
    }

    // GET: Permisos/Pendientes
    public async Task<IActionResult> Pendientes()
    {
        try
        {
            var permisosPendientes = await _nominaService.ObtenerPermisos(null, null, null, "solicitado");
            return View(permisosPendientes);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar permisos pendientes: {ex.Message}";
            return View(new List<GestionRRHH.Web.Models.Entities.Permiso>());
        }
    }
}