using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionRRHH.Web.Data;
using GestionRRHH.Web.Models.Entities;
using BCrypt.Net;

namespace GestionRRHH.Web.Controllers;

[Authorize(Roles = "admin")]
public class UsuariosController : Controller
{
    private readonly ApplicationDbContext _context;

    public UsuariosController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var usuarios = await _context.Usuarios
            .Include(u => u.IdEmpleadoNavigation)
            .ToListAsync();
        return View(usuarios);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var empleados = await _context.Empleados.ToListAsync();
        ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto");
        ViewBag.Roles = new SelectList(new[] { "admin", "rrhh", "supervisor", "empleado" });
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Usuario usuario)
    {
        if (ModelState.IsValid)
        {
            // Validación adicional para evitar duplicados
            if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario))
            {
                ModelState.AddModelError("NombreUsuario", "El nombre de usuario ya existe.");
                return View(usuario);
            }

            // Encriptar contraseña
            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);
            usuario.FechaCreacion = DateTime.Now;
            usuario.FechaModificacion = DateTime.Now;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        var empleados = await _context.Empleados.ToListAsync();
        ViewBag.Empleados = new SelectList(empleados, "Id", "NombreCompleto", usuario.IdEmpleado);
        ViewBag.Roles = new SelectList(new[] { "admin", "rrhh", "supervisor", "empleado" }, usuario.Rol);
        return View(usuario);
    }
}
