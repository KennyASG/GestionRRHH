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
        // Métodos para Vacaciones y Permisos
        Task<List<Vacacione>> ObtenerVacaciones(DateOnly? fechaInicio = null, DateOnly? fechaFin = null, int? empleadoId = null, string? estado = null);
        Task<Vacacione?> ObtenerVacacionesPorId(int id);
        Task<bool> SolicitarVacaciones(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, int diasVacaciones);
        Task<bool> AprobarVacaciones(int vacacionId);
        Task<bool> RechazarVacaciones(int vacacionId, string motivo);
        Task<List<Permiso>> ObtenerPermisos(DateOnly? fechaInicio = null, DateOnly? fechaFin = null, int? empleadoId = null, string? estado = null);
        Task<Permiso?> ObtenerPermisoPorId(int id);
        Task<bool> SolicitarPermiso(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, string tipo, string motivo, int dias, decimal? descuento = null);
        Task<bool> AprobarPermiso(int permisoId);
        Task<bool> RechazarPermiso(int permisoId);
        Task<int> CalcularDiasVacacionesDisponibles(int empleadoId);
        // Métodos para Suspensiones IGSS
        Task<List<SuspensionesIgss>> ObtenerSuspensionesIGSS(DateOnly? fechaInicio = null, DateOnly? fechaFin = null, int? empleadoId = null, string? estado = null);
        Task<SuspensionesIgss?> ObtenerSuspensionIGSSPorId(int id);
        Task<bool> CrearSuspensionIGSS(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, string tipoSuspension, string? observaciones);
        Task<bool> FinalizarSuspensionIGSS(int suspensionId);
        Task<bool> ActualizarSuspensionIGSS(int suspensionId, int diasPagadosIGSS, decimal montoSubsidio, string? observaciones);
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

        // ============ MÉTODOS PARA VACACIONES ============
        public async Task<List<Vacacione>> ObtenerVacaciones(DateOnly? fechaInicio = null, DateOnly? fechaFin = null, int? empleadoId = null, string? estado = null)
        {
            var query = _context.Vacaciones
                .Include(v => v.IdEmpleadoNavigation)
                .ThenInclude(e => e.Departamento)
                .Include(v => v.IdEmpleadoNavigation)
                .ThenInclude(e => e.Puesto)
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(v => v.FechaInicio >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(v => v.FechaFin <= fechaFin.Value);

            if (empleadoId.HasValue)
                query = query.Where(v => v.IdEmpleado == empleadoId.Value);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(v => v.Estado == estado);

            return await query.OrderByDescending(v => v.FechaCreacion)
                             .ThenBy(v => v.IdEmpleadoNavigation.NombreCompleto)
                             .ToListAsync();
        }

        public async Task<Vacacione?> ObtenerVacacionesPorId(int id)
        {
            return await _context.Vacaciones
                .Include(v => v.IdEmpleadoNavigation)
                .ThenInclude(e => e.Departamento)
                .Include(v => v.IdEmpleadoNavigation)
                .ThenInclude(e => e.Puesto)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<bool> SolicitarVacaciones(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, int diasVacaciones)
        {
            return await ProcesarVacaciones(empleadoId, fechaInicio, fechaFin, diasVacaciones);
        }

        public async Task<bool> AprobarVacaciones(int vacacionId)
        {
            try
            {
                var vacacion = await _context.Vacaciones.FindAsync(vacacionId);
                if (vacacion != null)
                {
                    vacacion.Estado = "aprobada";
                    vacacion.FechaModificacion = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al aprobar vacaciones: {ex.Message}");
            }
        }

        public async Task<bool> RechazarVacaciones(int vacacionId, string motivo)
        {
            try
            {
                var vacacion = await _context.Vacaciones.FindAsync(vacacionId);
                if (vacacion != null)
                {
                    vacacion.Estado = "rechazada";
                    vacacion.FechaModificacion = DateTime.Now;
                    // Nota: Podrías agregar un campo de motivo en la entidad si es necesario
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al rechazar vacaciones: {ex.Message}");
            }
        }

        public async Task<int> CalcularDiasVacacionesDisponibles(int empleadoId)
        {
            try
            {
                var empleado = await _context.Empleados.FindAsync(empleadoId);
                if (empleado == null) return 0;

                // Calcular antiguedad en años (15 días por año según legislación guatemalteca)
                var antiguedad = DateTime.Now.Year - empleado.FechaContratacion.Year;
                var diasTotales = antiguedad * 15;

                // Restar días ya tomados en el año actual
                var inicioAño = new DateOnly(DateTime.Now.Year, 1, 1);
                var finAño = new DateOnly(DateTime.Now.Year, 12, 31);

                var diasTomados = await _context.Vacaciones
                    .Where(v => v.IdEmpleado == empleadoId && 
                               v.FechaInicio >= inicioAño && 
                               v.FechaFin <= finAño &&
                               (v.Estado == "aprobada" || v.Estado == "pagada"))
                    .SumAsync(v => v.DiasTomados);

                return Math.Max(0, diasTotales - diasTomados);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular días disponibles: {ex.Message}");
            }
        }

        // ============ MÉTODOS PARA PERMISOS ============
        public async Task<List<Permiso>> ObtenerPermisos(DateOnly? fechaInicio = null, DateOnly? fechaFin = null, int? empleadoId = null, string? estado = null)
        {
            var query = _context.Permisos
                .Include(p => p.IdEmpleadoNavigation)
                .ThenInclude(e => e.Departamento)
                .Include(p => p.IdEmpleadoNavigation)
                .ThenInclude(e => e.Puesto)
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(p => p.FechaInicio >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(p => p.FechaFin <= fechaFin.Value);

            if (empleadoId.HasValue)
                query = query.Where(p => p.IdEmpleado == empleadoId.Value);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(p => p.Estado == estado);

            return await query.OrderByDescending(p => p.FechaCreacion)
                             .ThenBy(p => p.IdEmpleadoNavigation.NombreCompleto)
                             .ToListAsync();
        }

        public async Task<Permiso?> ObtenerPermisoPorId(int id)
        {
            return await _context.Permisos
                .Include(p => p.IdEmpleadoNavigation)
                .ThenInclude(e => e.Departamento)
                .Include(p => p.IdEmpleadoNavigation)
                .ThenInclude(e => e.Puesto)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> SolicitarPermiso(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, string tipo, string motivo, int dias, decimal? descuento = null)
        {
            try
            {
                var permiso = new Permiso
                {
                    IdEmpleado = empleadoId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    Tipo = tipo,
                    Motivo = motivo,
                    Dias = dias,
                    Descuento = descuento ?? 0,
                    Estado = "solicitado",
                    FechaCreacion = DateTime.Now,
                    FechaModificacion = DateTime.Now
                };

                _context.Permisos.Add(permiso);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al solicitar permiso: {ex.Message}");
            }
        }

        public async Task<bool> AprobarPermiso(int permisoId)
        {
            try
            {
                var permiso = await _context.Permisos.FindAsync(permisoId);
                if (permiso != null)
                {
                    permiso.Estado = "aprobado";
                    permiso.FechaModificacion = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al aprobar permiso: {ex.Message}");
            }
        }

        public async Task<bool> RechazarPermiso(int permisoId)
        {
            try
            {
                var permiso = await _context.Permisos.FindAsync(permisoId);
                if (permiso != null)
                {
                    permiso.Estado = "rechazado";
                    permiso.FechaModificacion = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al rechazar permiso: {ex.Message}");
            }
        }
        
        // ============ MÉTODOS PARA SUSPENSIONES IGSS ============
        public async Task<List<SuspensionesIgss>> ObtenerSuspensionesIGSS(DateOnly? fechaInicio = null, DateOnly? fechaFin = null, int? empleadoId = null, string? estado = null)
        {
            var query = _context.SuspensionesIgsses
                .Include(s => s.IdEmpleadoNavigation)
                .ThenInclude(e => e.Departamento)
                .Include(s => s.IdEmpleadoNavigation)
                .ThenInclude(e => e.Puesto)
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(s => s.FechaInicio >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(s => s.FechaFin <= fechaFin.Value);

            if (empleadoId.HasValue)
                query = query.Where(s => s.IdEmpleado == empleadoId.Value);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(s => s.Estado == estado);

            return await query.OrderByDescending(s => s.FechaCreacion)
                             .ThenBy(s => s.IdEmpleadoNavigation.NombreCompleto)
                             .ToListAsync();
        }

        public async Task<SuspensionesIgss?> ObtenerSuspensionIGSSPorId(int id)
        {
            return await _context.SuspensionesIgsses
                .Include(s => s.IdEmpleadoNavigation)
                .ThenInclude(e => e.Departamento)
                .Include(s => s.IdEmpleadoNavigation)
                .ThenInclude(e => e.Puesto)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> CrearSuspensionIGSS(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, string tipoSuspension, string? observaciones)
        {
            return await ProcesarSuspensionIGSS(empleadoId, fechaInicio, fechaFin, tipoSuspension, observaciones);
        }

        public async Task<bool> FinalizarSuspensionIGSS(int suspensionId)
        {
            try
            {
                var suspension = await _context.SuspensionesIgsses.FindAsync(suspensionId);
                if (suspension != null)
                {
                    suspension.Estado = "finalizada";
                    suspension.FechaModificacion = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al finalizar suspensión IGSS: {ex.Message}");
            }
        }

        public async Task<bool> ActualizarSuspensionIGSS(int suspensionId, int diasPagadosIGSS, decimal montoSubsidio, string? observaciones)
        {
            try
            {
                var suspension = await _context.SuspensionesIgsses.FindAsync(suspensionId);
                if (suspension != null)
                {
                    suspension.DiasPagadosIgss = diasPagadosIGSS;
                    suspension.MontoSubsidio = montoSubsidio;
                    suspension.Observaciones = observaciones;
                    suspension.FechaModificacion = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar suspensión IGSS: {ex.Message}");
            }
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