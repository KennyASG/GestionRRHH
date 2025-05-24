using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GestionRRHH.Web.Data;
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

    public async Task<IActionResult> Index()
    {
        var empleados = await _context.Empleados
            .Include(e => e.Departamento)
            .Include(e => e.Puesto)
            .ToListAsync();
        return View(empleados);
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