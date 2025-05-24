using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GestionRRHH.Web.Data;
using GestionRRHH.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionRRHH.Web.Controllers;

[Authorize]
public class LiquidacionesController : Controller
{
    private readonly ApplicationDbContext _context;

    public LiquidacionesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Liquidaciones
    // REEMPLAZA el método Index en LiquidacionesController.cs

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

        // SOLUCIÓN: Construir la consulta paso a paso para evitar el error de traducción
        var query = _context.Liquidaciones
            .Include(l => l.IdEmpleadoNavigation)
            .ThenInclude(e => e.Departamento)
            .Include(l => l.IdEmpleadoNavigation)
            .ThenInclude(e => e.Puesto)
            .AsQueryable();

        // Aplicar filtros de fecha
        if (fechaInicio.HasValue)
            query = query.Where(l => l.FechaBaja >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(l => l.FechaBaja <= fechaFin.Value);

        // SOLUCIÓN: Aplicar filtro de empleado de manera más simple
        if (empleadoId.HasValue && empleadoId.Value > 0)
            query = query.Where(l => l.IdEmpleado == empleadoId.Value);

        // Aplicar filtro de estado
        if (!string.IsNullOrEmpty(estado))
            query = query.Where(l => l.Estado == estado);

        // SOLUCIÓN: Ejecutar la consulta principal primero
        var liquidaciones = await query
            .OrderByDescending(l => l.FechaCreacion)
            .ToListAsync();

        // Cargar empleados para filtros - CONSULTA SEPARADA para evitar conflictos
        var empleadosParaFiltro = await _context.Empleados
            .Where(e => e.Estado == "activo" || 
                       _context.Liquidaciones.Any(l => l.IdEmpleado == e.Id))
            .OrderBy(e => e.NombreCompleto)
            .ToListAsync();

        // Si hay error en la consulta de empleados, usar consulta más simple
        if (empleadosParaFiltro == null || !empleadosParaFiltro.Any())
        {
            empleadosParaFiltro = await _context.Empleados
                .Where(e => e.Estado == "activo")
                .OrderBy(e => e.NombreCompleto)
                .ToListAsync();
        }

        ViewBag.FechaInicio = fechaInicio;
        ViewBag.FechaFin = fechaFin;
        ViewBag.EmpleadoId = empleadoId;
        ViewBag.Estado = estado;
        ViewBag.Empleados = new SelectList(empleadosParaFiltro, "Id", "NombreCompleto", empleadoId);

        return View(liquidaciones);
    }
    catch (Exception ex)
    {
        TempData["Error"] = $"Error al cargar liquidaciones: {ex.Message}";
        
        // En caso de error, devolver vista vacía con empleados básicos
        var empleadosBasicos = await _context.Empleados
            .Where(e => e.Estado == "activo")
            .Select(e => new { e.Id, e.NombreCompleto })
            .ToListAsync();
            
        ViewBag.FechaInicio = fechaInicio;
        ViewBag.FechaFin = fechaFin;
        ViewBag.EmpleadoId = empleadoId;
        ViewBag.Estado = estado;
        ViewBag.Empleados = new SelectList(empleadosBasicos, "Id", "NombreCompleto", empleadoId);
        
        return View(new List<Liquidacione>());
    }
}

    // GET: Liquidaciones/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var liquidacion = await _context.Liquidaciones
                .Include(l => l.IdEmpleadoNavigation)
                .ThenInclude(e => e.Departamento)
                .Include(l => l.IdEmpleadoNavigation)
                .ThenInclude(e => e.Puesto)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (liquidacion == null)
            {
                TempData["Error"] = "Liquidación no encontrada.";
                return RedirectToAction("Index");
            }

            return View(liquidacion);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar detalles: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // GET: Liquidaciones/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        try
        {
            var empleadosActivos = await _context.Empleados
                .Where(e => e.Estado == "activo")
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
                .OrderBy(e => e.NombreCompleto)
                .ToListAsync();

            ViewBag.Empleados = new SelectList(empleadosActivos, "Id", "NombreCompleto");
            
            return View();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar formulario: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // POST: Liquidaciones/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int empleadoId, DateOnly fechaBaja, string motivoBaja)
    {
        try
        {
            if (empleadoId <= 0)
            {
                TempData["Error"] = "Debe seleccionar un empleado.";
                await CargarEmpleadosParaVista();
                return View();
            }

            if (fechaBaja == default)
            {
                TempData["Error"] = "La fecha de baja es obligatoria.";
                await CargarEmpleadosParaVista();
                return View();
            }

            if (fechaBaja < DateOnly.FromDateTime(DateTime.Now.AddDays(-30)))
            {
                TempData["Error"] = "La fecha de baja no puede ser anterior a 30 días.";
                await CargarEmpleadosParaVista();
                return View();
            }

            var empleado = await _context.Empleados.FindAsync(empleadoId);
            if (empleado == null)
            {
                TempData["Error"] = "Empleado no encontrado.";
                await CargarEmpleadosParaVista();
                return View();
            }

            if (empleado.Estado != "activo")
            {
                TempData["Error"] = "El empleado ya no está activo.";
                await CargarEmpleadosParaVista();
                return View();
            }

            // Verificar si ya tiene liquidación
            var liquidacionExistente = await _context.Liquidaciones
                .FirstOrDefaultAsync(l => l.IdEmpleado == empleadoId);

            if (liquidacionExistente != null)
            {
                TempData["Error"] = "Este empleado ya tiene una liquidación registrada.";
                await CargarEmpleadosParaVista();
                return View();
            }

            // Calcular liquidación
            var liquidacion = await CalcularLiquidacion(empleado, fechaBaja);

            // Guardar liquidación
            _context.Liquidaciones.Add(liquidacion);
            
            // Inactivar empleado
            empleado.Estado = "inactivo";
            empleado.FechaModificacion = DateTime.Now;
            
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Liquidación calculada exitosamente. Total: {liquidacion.TotalLiquidacion:C}";
            return RedirectToAction("Details", new { id = liquidacion.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al crear liquidación: {ex.Message}";
            await CargarEmpleadosParaVista();
            return View();
        }
    }

    // POST: Liquidaciones/Pagar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pagar(int id)
    {
        try
        {
            var liquidacion = await _context.Liquidaciones.FindAsync(id);
            if (liquidacion != null && liquidacion.Estado == "calculada")
            {
                liquidacion.Estado = "pagada";
                liquidacion.FechaModificacion = DateTime.Now;
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Liquidación marcada como pagada.";
            }
            else
            {
                TempData["Error"] = "No se puede marcar como pagada esta liquidación.";
            }

            return RedirectToAction("Details", new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Details", new { id });
        }
    }

    // Método privado para calcular liquidación
    private async Task<Liquidacione> CalcularLiquidacion(Empleado empleado, DateOnly fechaBaja)
    {
        // Calcular antigüedad
        var antiguedad = fechaBaja.Year - empleado.FechaContratacion.Year;
        if (fechaBaja.DayOfYear < empleado.FechaContratacion.DayOfYear)
            antiguedad--;

        // Calcular vacaciones pendientes (15 días por año - ya tomadas)
        var vacacionesTotales = antiguedad * 15;
        var vacacionesTomadas = await _context.Vacaciones
            .Where(v => v.IdEmpleado == empleado.Id && v.Estado == "pagada")
            .SumAsync(v => v.DiasTomados);
        
        var vacacionesPendientes = Math.Max(0, vacacionesTotales - vacacionesTomadas);

        // Calcular pago de vacaciones (salario diario * días pendientes)
        var salarioDiario = empleado.SalarioBase / 30;
        var pagoVacaciones = vacacionesPendientes * salarioDiario;

        // Calcular indemnización (1 mes por año trabajado)
        var indemnizacion = antiguedad * empleado.SalarioBase;

        // Aguinaldo proporcional (meses trabajados en el año / 12 * salario)
        var mesesTrabajadosEnAño = fechaBaja.Month;
        var aguinaldoProporcional = (mesesTrabajadosEnAño / 12.0m) * empleado.SalarioBase;

        // Bono 14 proporcional
        var bono14Proporcional = (mesesTrabajadosEnAño / 12.0m) * empleado.SalarioBase;

        // Total liquidación
        var totalLiquidacion = pagoVacaciones + indemnizacion + aguinaldoProporcional + bono14Proporcional;

        return new Liquidacione
        {
            IdEmpleado = empleado.Id,
            FechaBaja = fechaBaja,
            AntiguedadAnios = antiguedad,
            VacacionesPendientes = vacacionesPendientes,
            PagoVacaciones = pagoVacaciones,
            Indemnizacion = indemnizacion,
            AguinaldoProporcional = aguinaldoProporcional,
            Bono14Proporcional = bono14Proporcional,
            TotalLiquidacion = totalLiquidacion,
            Estado = "calculada",
            FechaCreacion = DateTime.Now,
            FechaModificacion = DateTime.Now
        };
    }

    private async Task CargarEmpleadosParaVista()
    {
        var empleados = await _context.Empleados
            .Where(e => e.Estado == "activo")
            .OrderBy(e => e.NombreCompleto)
            .ToListAsync();
        ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
    }
}