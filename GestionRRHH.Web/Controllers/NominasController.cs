using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionRRHH.Web.Services;

namespace GestionRRHH.Web.Controllers;

[Authorize]
public class NominasController : Controller
{
    private readonly INominaService _nominaService;

    public NominasController(INominaService nominaService)
    {
        _nominaService = nominaService;
    }

    // GET: Nominas
    public async Task<IActionResult> Index()
    {
        try
        {
            var nominas = await _nominaService.ObtenerNominas();
            return View(nominas);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar nóminas: {ex.Message}";
            return View(new List<GestionRRHH.Web.Models.Entities.Nomina>());
        }
    }

    // GET: Nominas/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Nominas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DateOnly fechaInicio, DateOnly fechaFin, string tipo)
    {
        try
        {
            if (fechaInicio == default || fechaFin == default || string.IsNullOrEmpty(tipo))
            {
                TempData["Error"] = "Todos los campos son obligatorios.";
                return View();
            }

            if (fechaInicio >= fechaFin)
            {
                TempData["Error"] = "La fecha de fin debe ser posterior a la fecha de inicio.";
                return View();
            }

            var usuarioCreacion = User.Identity?.Name ?? "Sistema";
            var nominaId = await _nominaService.CrearNomina(fechaInicio, fechaFin, tipo, usuarioCreacion);

            if (nominaId > 0)
            {
                TempData["Success"] = "Nómina creada exitosamente.";
                return RedirectToAction("Details", new { id = nominaId });
            }
            else
            {
                TempData["Error"] = "Error al crear la nómina.";
                return View();
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View();
        }
    }

    // GET: Nominas/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var nomina = await _nominaService.ObtenerNominaPorId(id);
            if (nomina == null)
            {
                TempData["Error"] = "Nómina no encontrada.";
                return RedirectToAction("Index");
            }

            return View(nomina);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar detalles: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // POST: Nominas/Procesar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Procesar(int id)
    {
        try
        {
            var resultado = await _nominaService.ProcesarNomina(id);
            
            if (resultado)
            {
                TempData["Success"] = "Nómina procesada exitosamente. Todos los cálculos han sido aplicados.";
            }
            else
            {
                TempData["Error"] = "Error al procesar la nómina.";
            }

            return RedirectToAction("Details", new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Details", new { id });
        }
    }

    // GET: Nominas/Asistencias
    [HttpGet]
    public IActionResult Asistencias()
    {
        return View();
    }

    // POST: Nominas/RegistrarAsistencia
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarAsistencia(int empleadoId, DateOnly fecha, TimeOnly horaEntrada, TimeOnly? horaSalida, string? observaciones)
    {
        try
        {
            var resultado = await _nominaService.RegistrarAsistencia(empleadoId, fecha, horaEntrada, horaSalida, observaciones);
            
            if (resultado)
            {
                TempData["Success"] = "Asistencia registrada exitosamente.";
            }
            else
            {
                TempData["Error"] = "Error al registrar la asistencia.";
            }

            return RedirectToAction("Asistencias");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Asistencias");
        }
    }

    // GET: Nominas/Vacaciones
    [HttpGet]
    public IActionResult Vacaciones()
    {
        return View();
    }

    // POST: Nominas/ProcesarVacaciones
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcesarVacaciones(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, int diasVacaciones)
    {
        try
        {
            var resultado = await _nominaService.ProcesarVacaciones(empleadoId, fechaInicio, fechaFin, diasVacaciones);
            
            if (resultado)
            {
                TempData["Success"] = "Vacaciones procesadas exitosamente.";
            }
            else
            {
                TempData["Error"] = "Error al procesar las vacaciones.";
            }

            return RedirectToAction("Vacaciones");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Vacaciones");
        }
    }

    // GET: Nominas/SuspensionesIGSS
    [HttpGet]
    public IActionResult SuspensionesIGSS()
    {
        return View();
    }

    // POST: Nominas/ProcesarSuspension
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcesarSuspension(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, string tipoSuspension, string? observaciones)
    {
        try
        {
            var resultado = await _nominaService.ProcesarSuspensionIGSS(empleadoId, fechaInicio, fechaFin, tipoSuspension, observaciones);
            
            if (resultado)
            {
                TempData["Success"] = "Suspensión IGSS procesada exitosamente.";
            }
            else
            {
                TempData["Error"] = "Error al procesar la suspensión.";
            }

            return RedirectToAction("SuspensionesIGSS");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("SuspensionesIGSS");
        }
    }
}