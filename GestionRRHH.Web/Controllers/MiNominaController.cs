using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionRRHH.Web.Data;

[Authorize(Roles = "empleado")]
public class MiNominaController : Controller
{
    private readonly ApplicationDbContext _context;

    public MiNominaController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Leer el EmpleadoID desde los claims
        var empleadoClaim = User.FindFirst("EmpleadoID")?.Value;

        if (string.IsNullOrEmpty(empleadoClaim))
            return Unauthorized();

        int empleadoId = int.Parse(empleadoClaim);

        var nominas = await _context.DetalleNominas
            .Include(dn => dn.IdNominaNavigation)
            .Where(dn => dn.IdEmpleado == empleadoId)
            .OrderByDescending(dn => dn.IdNominaNavigation.FechaInicio)
            .ToListAsync();

        return View(nominas);
    }
}