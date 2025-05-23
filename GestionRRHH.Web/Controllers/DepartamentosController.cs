using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionRRHH.Web.Data;
using GestionRRHH.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionRRHH.Web.Controllers;

[Authorize]
public class DepartamentosController : Controller
{
    private readonly ApplicationDbContext _context;

    public DepartamentosController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var departamentos = await _context.Departamentos
            .Include(d => d.Responsable)
            .ToListAsync();
        return View(departamentos);
    }
    
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        // Cargar empleados para dropdown de responsable
        ViewBag.Empleados = await _context.Empleados
            .Where(e => e.Estado == "activo")
            .Select(e => new { e.Id, e.NombreCompleto })
            .ToListAsync();
        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Departamento departamento)
    {
        if (ModelState.IsValid)
        {
            departamento.FechaCreacion = DateTime.Now;
            departamento.FechaModificacion = DateTime.Now;
            _context.Departamentos.Add(departamento);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        // Recargar empleados si hay error
        ViewBag.Empleados = await _context.Empleados
            .Where(e => e.Estado == "activo")
            .Select(e => new { e.Id, e.NombreCompleto })
            .ToListAsync();
        
        return View(departamento);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var departamento = await _context.Departamentos.FindAsync(id);
        if (departamento == null)
            return NotFound();
        
        ViewBag.Empleados = await _context.Empleados
            .Where(e => e.Estado == "activo")
            .Select(e => new { e.Id, e.NombreCompleto })
            .ToListAsync();
        
        return View(departamento);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Departamento departamento)
    {
        if (id != departamento.Id)
            return NotFound();
        
        if (ModelState.IsValid)
        {
            departamento.FechaModificacion = DateTime.Now;
            _context.Update(departamento);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        ViewBag.Empleados = await _context.Empleados
            .Where(e => e.Estado == "activo")
            .Select(e => new { e.Id, e.NombreCompleto })
            .ToListAsync();
        
        return View(departamento);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var departamento = await _context.Departamentos.FindAsync(id);
        if (departamento != null)
        {
            _context.Departamentos.Remove(departamento);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}