using Microsoft.AspNetCore.Mvc;
using GestionRRHH.Web.Data;
using Microsoft.AspNetCore.Authorization;

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
        var empleados = _context.Empleados.ToList();
        return View(empleados);
    }
}