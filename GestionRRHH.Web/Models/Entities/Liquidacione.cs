using System;
using System.Collections.Generic;

namespace GestionRRHH.Web.Models.Entities;

public partial class Liquidacione
{
    public int Id { get; set; }

    public int IdEmpleado { get; set; }

    public DateOnly FechaBaja { get; set; }

    public int AntiguedadAnios { get; set; }

    public int? VacacionesPendientes { get; set; }

    public decimal? PagoVacaciones { get; set; }

    public decimal? Indemnizacion { get; set; }

    public decimal? Bono14Proporcional { get; set; }

    public decimal? AguinaldoProporcional { get; set; }

    public decimal TotalLiquidacion { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
}
