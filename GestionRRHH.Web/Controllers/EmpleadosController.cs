using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GestionRRHH.Web.Data;
using GestionRRHH.Web.Models;
using GestionRRHH.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionRRHH.Web.Controllers;

[Authorize]
public class EmpleadosController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmpleadosController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(EmpleadoFilter filter)
    {
        var query = _context.Empleados
            .Include(e => e.Departamento)
            .Include(e => e.Puesto)
            .AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrEmpty(filter.Estado))
            query = query.Where(e => e.Estado == filter.Estado);

        if (!string.IsNullOrEmpty(filter.Nombre))
            query = query.Where(e => e.NombreCompleto.Contains(filter.Nombre));

        if (!string.IsNullOrEmpty(filter.Departamento))
            query = query.Where(e => e.Departamento.Nombre == filter.Departamento);

        if (!string.IsNullOrEmpty(filter.TipoNomina))
            query = query.Where(e => e.TipoNomina == filter.TipoNomina);

        // Contar total antes de paginar
        var totalItems = await query.CountAsync();

        // Aplicar paginación
        var empleados = await query
            .OrderBy(e => e.NombreCompleto)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var paginatedList = new PaginatedList<Empleado>(empleados, totalItems, filter.Page, filter.PageSize);

        // Pasar filtro actual a la vista
        ViewBag.CurrentFilter = filter;
    
        // Cargar datos para filtros
        ViewBag.Departamentos = await _context.Departamentos.Select(d => d.Nombre).Distinct().ToListAsync();

        return View(paginatedList);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadDropdowns();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Empleado empleado)
    {
        if (ModelState.IsValid)
        {
            empleado.FechaCreacion = DateTime.Now;
            empleado.FechaModificacion = DateTime.Now;
            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Empleado creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        
        await LoadDropdowns();
        return View(empleado);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id);
        if (empleado == null)
            return NotFound();
        
        await LoadDropdowns();
        return View(empleado);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Empleado empleado)
    {
        if (id != empleado.Id)
            return NotFound();
        
        if (ModelState.IsValid)
        {
            empleado.FechaModificacion = DateTime.Now;
            _context.Update(empleado);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Empleado actualizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        
        await LoadDropdowns();
        return View(empleado);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id);
        if (empleado != null)
        {
            empleado.Estado = "inactivo";
            empleado.FechaModificacion = DateTime.Now;
            _context.Update(empleado);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Empleado desactivado exitosamente.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDropdowns()
    {
        ViewBag.Departamentos = new SelectList(
            await _context.Departamentos.ToListAsync(), 
            "Id", "Nombre");
        
        ViewBag.Puestos = new SelectList(
            await _context.Puestos.ToListAsync(), 
            "Id", "Nombre");
    }
}