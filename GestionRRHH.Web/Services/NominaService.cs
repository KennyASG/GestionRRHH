using GestionRRHH.Web.Data;
using GestionRRHH.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace GestionRRHH.Web.Services
{
    public interface INominaService
    {
        Task<int> CrearNomina(DateOnly fechaInicio, DateOnly fechaFin, string tipo, string usuarioCreacion);
        Task<bool> ProcesarNomina(int nominaId);
        Task<List<Nomina>> ObtenerNominas();
        Task<Nomina?> ObtenerNominaPorId(int id);
        Task<bool> RegistrarAsistencia(int empleadoId, DateOnly fecha, TimeOnly horaEntrada, TimeOnly? horaSalida, string? observaciones);
        Task<bool> ProcesarVacaciones(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, int diasVacaciones);
        Task<bool> ProcesarSuspensionIGSS(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, string tipoSuspension, string? observaciones);
        Task<List<Asistencia>> ObtenerAsistencias(DateOnly? fechaInicio = null, DateOnly? fechaFin = null, int? empleadoId = null);
        Task<List<Empleado>> ObtenerEmpleadosActivos();
        Task<Asistencia?> ObtenerAsistenciaPorId(int id);
        Task<bool> EliminarAsistencia(int id);
        Task<object> ObtenerReporteAsistencias(DateOnly? fechaInicio = null, DateOnly? fechaFin = null, int? empleadoId = null);
    }

    public class NominaService : INominaService
    {
        private readonly ApplicationDbContext _context;

        public NominaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CrearNomina(DateOnly fechaInicio, DateOnly fechaFin, string tipo, string usuarioCreacion)
        {
            try
            {
                var parameters = new[]
                {
                    new MySqlParameter("@p_FechaInicio", fechaInicio.ToString("yyyy-MM-dd")),
                    new MySqlParameter("@p_FechaFin", fechaFin.ToString("yyyy-MM-dd")),
                    new MySqlParameter("@p_Tipo", tipo),
                    new MySqlParameter("@p_UsuarioCreacion", usuarioCreacion)
                };

                var result = await _context.Database
                    .SqlQueryRaw<NominaResultado>("CALL sp_CrearNomina(@p_FechaInicio, @p_FechaFin, @p_Tipo, @p_UsuarioCreacion)", parameters)
                    .ToListAsync();

                return result.FirstOrDefault()?.NominaID ?? 0;
            }
            catch (Exception ex)
            {
                // Log error
                throw new Exception($"Error al crear nómina: {ex.Message}");
            }
        }

        public async Task<bool> ProcesarNomina(int nominaId)
        {
            try
            {
                var parameter = new MySqlParameter("@p_NominaID", nominaId);
                
                var result = await _context.Database
                    .SqlQueryRaw<NominaProcesamientoResultado>("CALL sp_CalcularNomina(@p_NominaID)", parameter)
                    .ToListAsync();

                return result.Any();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al procesar nómina: {ex.Message}");
            }
        }

        public async Task<List<Nomina>> ObtenerNominas()
        {
            return await _context.Nominas
                .Include(n => n.DetalleNominas)
                .ThenInclude(d => d.IdEmpleadoNavigation)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
        }

        public async Task<Nomina?> ObtenerNominaPorId(int id)
        {
            return await _context.Nominas
                .Include(n => n.DetalleNominas)
                .ThenInclude(d => d.IdEmpleadoNavigation)
                .ThenInclude(e => e.Puesto)
                .Include(n => n.DetalleNominas)
                .ThenInclude(d => d.IdEmpleadoNavigation)
                .ThenInclude(e => e.Departamento)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<bool> RegistrarAsistencia(int empleadoId, DateOnly fecha, TimeOnly horaEntrada, TimeOnly? horaSalida, string? observaciones)
        {
            try
            {
                var parameters = new[]
                {
                    new MySqlParameter("@p_EmpleadoID", empleadoId),
                    new MySqlParameter("@p_Fecha", fecha.ToString("yyyy-MM-dd")),
                    new MySqlParameter("@p_HoraEntrada", horaEntrada.ToString("HH:mm:ss")),
                    new MySqlParameter("@p_HoraSalida", horaSalida?.ToString("HH:mm:ss") ?? "23:59:59"),
                    new MySqlParameter("@p_Observaciones", observaciones ?? "")
                };

                var result = await _context.Database
                    .SqlQueryRaw<AsistenciaResultado>("CALL sp_RegistrarAsistencia(@p_EmpleadoID, @p_Fecha, @p_HoraEntrada, @p_HoraSalida, @p_Observaciones)", parameters)
                    .ToListAsync();

                return result.Any();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al registrar asistencia: {ex.Message}");
            }
        }

        public async Task<bool> ProcesarVacaciones(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, int diasVacaciones)
        {
            try
            {
                var parameters = new[]
                {
                    new MySqlParameter("@p_EmpleadoID", empleadoId),
                    new MySqlParameter("@p_FechaInicio", fechaInicio.ToString("yyyy-MM-dd")),
                    new MySqlParameter("@p_FechaFin", fechaFin.ToString("yyyy-MM-dd")),
                    new MySqlParameter("@p_DiasVacaciones", diasVacaciones)
                };

                var result = await _context.Database
                    .SqlQueryRaw<VacacionesResultado>("CALL sp_ProcesarVacaciones(@p_EmpleadoID, @p_FechaInicio, @p_FechaFin, @p_DiasVacaciones)", parameters)
                    .ToListAsync();

                return result.Any();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al procesar vacaciones: {ex.Message}");
            }
        }

        public async Task<bool> ProcesarSuspensionIGSS(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, string tipoSuspension, string? observaciones)
        {
            try
            {
                var parameters = new[]
                {
                    new MySqlParameter("@p_EmpleadoID", empleadoId),
                    new MySqlParameter("@p_FechaInicio", fechaInicio.ToString("yyyy-MM-dd")),
                    new MySqlParameter("@p_FechaFin", fechaFin.ToString("yyyy-MM-dd")),
                    new MySqlParameter("@p_TipoSuspension", tipoSuspension),
                    new MySqlParameter("@p_Observaciones", observaciones ?? "")
                };

                var result = await _context.Database
                    .SqlQueryRaw<SuspensionResultado>("CALL sp_ProcesarSuspensionIGSS(@p_EmpleadoID, @p_FechaInicio, @p_FechaFin, @p_TipoSuspension, @p_Observaciones)", parameters)
                    .ToListAsync();

                return result.Any();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al procesar suspensión IGSS: {ex.Message}");
            }
        }

        // Métodos adicionales para Asistencias
        public async Task<List<Asistencia>> ObtenerAsistencias(DateOnly? fechaInicio = null, DateOnly? fechaFin = null, int? empleadoId = null)
        {
            var query = _context.Asistencias
                .Include(a => a.IdEmpleadoNavigation)
                .ThenInclude(e => e.Departamento)
                .Include(a => a.IdEmpleadoNavigation)
                .ThenInclude(e => e.Puesto)
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(a => a.Fecha >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(a => a.Fecha <= fechaFin.Value);

            if (empleadoId.HasValue)
                query = query.Where(a => a.IdEmpleado == empleadoId.Value);

            return await query.OrderByDescending(a => a.Fecha)
                .ThenBy(a => a.IdEmpleadoNavigation.NombreCompleto)
                .ToListAsync();
        }

        public async Task<List<Empleado>> ObtenerEmpleadosActivos()
        {
            return await _context.Empleados
                .Where(e => e.Estado == "activo")
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
                .OrderBy(e => e.NombreCompleto)
                .ToListAsync();
        }

        public async Task<Asistencia?> ObtenerAsistenciaPorId(int id)
        {
            return await _context.Asistencias
                .Include(a => a.IdEmpleadoNavigation)
                .ThenInclude(e => e.Departamento)
                .Include(a => a.IdEmpleadoNavigation)
                .ThenInclude(e => e.Puesto)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> EliminarAsistencia(int id)
        {
            try
            {
                var asistencia = await _context.Asistencias.FindAsync(id);
                if (asistencia != null)
                {
                    _context.Asistencias.Remove(asistencia);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar asistencia: {ex.Message}");
            }
        }

        public async Task<object> ObtenerReporteAsistencias(DateOnly? fechaInicio = null, DateOnly? fechaFin = null, int? empleadoId = null)
        {
            var asistencias = await ObtenerAsistencias(fechaInicio, fechaFin, empleadoId);
    
            // Cálculos para el reporte
            var totalRegistros = asistencias.Count;
            var horasExtraTotal = asistencias.Sum(a => a.HorasExtra ?? 0);
            var horasTrabajadasTotal = asistencias.Sum(a => a.HorasTrabajadas ?? 0);
            var empleadosUnicos = asistencias.Select(a => a.IdEmpleado).Distinct().Count();
    
            // Resumen por empleado
            var resumenPorEmpleado = asistencias.GroupBy(a => new { 
                a.IdEmpleado, 
                a.IdEmpleadoNavigation.NombreCompleto,
                Departamento = a.IdEmpleadoNavigation.Departamento?.Nombre,
                Puesto = a.IdEmpleadoNavigation.Puesto?.Nombre
            }).Select(g => new {
                EmpleadoId = g.Key.IdEmpleado,
                NombreCompleto = g.Key.NombreCompleto,
                Departamento = g.Key.Departamento,
                Puesto = g.Key.Puesto,
                TotalDias = g.Count(),
                TotalHorasTrabajadas = g.Sum(a => a.HorasTrabajadas ?? 0),
                TotalHorasExtra = g.Sum(a => a.HorasExtra ?? 0),
                PromedioHorasDiarias = g.Count() > 0 ? g.Sum(a => a.HorasTrabajadas ?? 0) / g.Count() : 0
            }).OrderBy(x => x.NombreCompleto).ToList();

            return new {
                Asistencias = asistencias,
                TotalRegistros = totalRegistros,
                HorasExtraTotal = horasExtraTotal,
                HorasTrabajadasTotal = horasTrabajadasTotal,
                EmpleadosUnicos = empleadosUnicos,
                ResumenPorEmpleado = resumenPorEmpleado
            };
        }
        
        
    }

    // Clases para los resultados de los SP
    public class NominaResultado
    {
        public int NominaID { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }

    public class NominaProcesamientoResultado
    {
        public int NominaID { get; set; }
        public string Resultado { get; set; } = string.Empty;
        public int EmpleadosProcesados { get; set; }
        public decimal TotalNomina { get; set; }
    }

    public class AsistenciaResultado
    {
        public string Resultado { get; set; } = string.Empty;
    }

    public class VacacionesResultado
    {
        public string Mensaje { get; set; } = string.Empty;
        public decimal MontoAPagar { get; set; }
    }

    public class SuspensionResultado
    {
        public string Mensaje { get; set; } = string.Empty;
        public int DiasQuePageIGSS { get; set; }
        public decimal MontoSubsidio { get; set; }
    }
    
    
}