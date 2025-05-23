using System;
using System.Collections.Generic;

namespace GestionRRHH.Web.Models.Entities;

public partial class Asistencia
{
    public int Id { get; set; }

    public int IdEmpleado { get; set; }

    public DateOnly Fecha { get; set; }

    public TimeOnly? HoraEntrada { get; set; }

    public TimeOnly? HoraSalida { get; set; }

    public decimal? HorasTrabajadas { get; set; }

    public decimal? HorasExtra { get; set; }

    public string? Observaciones { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
}
