using System;
using System.Collections.Generic;

namespace GestionRRHH.Web.Models.Entities;

public partial class Prestacione
{
    public int Id { get; set; }

    public int IdEmpleado { get; set; }

    public string Tipo { get; set; } = null!;

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public decimal Monto { get; set; }

    public string? Estado { get; set; }

    public string? Comentarios { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
}
