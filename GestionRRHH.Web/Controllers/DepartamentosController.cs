using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionRRHH.Web.Data;
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
}