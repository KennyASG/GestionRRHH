using System;
using System.Collections.Generic;

namespace GestionRRHH.Web.Models.Entities;

public partial class Vacacione
{
    public int Id { get; set; }

    public int IdEmpleado { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public int DiasTomados { get; set; }

    public decimal? PromedioBonosExtras { get; set; }

    public decimal PagoTotal { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
}
