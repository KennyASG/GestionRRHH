using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionRRHH.Web.Data;
using GestionRRHH.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionRRHH.Web.Controllers;

[Authorize]
public class PuestosController : Controller
{
    private readonly ApplicationDbContext _context;

    public PuestosController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var puestos = await _context.Puestos
            .Include(p => p.Empleados)
            .ToListAsync();
        return View(puestos);
    }
    
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Puesto puesto)
    {
        if (ModelState.IsValid)
        {
            puesto.FechaCreacion = DateTime.Now;
            puesto.FechaModificacion = DateTime.Now;
            _context.Puestos.Add(puesto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(puesto);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var puesto = await _context.Puestos.FindAsync(id);
        if (puesto == null)
            return NotFound();
    
        return View(puesto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Puesto puesto)
    {
        if (id != puesto.Id)
            return NotFound();
    
        if (ModelState.IsValid)
        {
            puesto.FechaModificacion = DateTime.Now;
            _context.Update(puesto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(puesto);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var puesto = await _context.Puestos
            .Include(p => p.Empleados)
            .FirstOrDefaultAsync(p => p.Id == id);
    
        if (puesto != null)
        {
            // Verificar si tiene empleados asignados
            if (puesto.Empleados.Any())
            {
                TempData["Error"] = $"No se puede eliminar el puesto '{puesto.Nombre}' porque tiene {puesto.Empleados.Count} empleados asignados.";
                return RedirectToAction(nameof(Index));
            }
        
            _context.Puestos.Remove(puesto);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Puesto eliminado exitosamente.";
        }
    
        return RedirectToAction(nameof(Index));
    }
}