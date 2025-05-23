using System;
using System.Collections.Generic;

namespace GestionRRHH.Web.Models.Entities;

public partial class DetalleNomina
{
    public int Id { get; set; }

    public int IdNomina { get; set; }

    public int IdEmpleado { get; set; }

    public int? DiasLaborados { get; set; }

    public decimal SalarioBase { get; set; }

    public int? HorasExtra { get; set; }

    public decimal? MontoHorasExtra { get; set; }

    public decimal? BonificacionIncentivo { get; set; }

    public decimal? Bonificaciones { get; set; }

    public decimal? Comisiones { get; set; }

    public decimal? OtrosIngresos { get; set; }

    public decimal? Aguinaldo { get; set; }

    public decimal? Bono14 { get; set; }

    public decimal? TotalIngresos { get; set; }

    public decimal? Igss { get; set; }

    public decimal? Isr { get; set; }

    public decimal? DescuentosJudiciales { get; set; }

    public decimal? OtrosDescuentos { get; set; }

    public decimal? TotalDescuentos { get; set; }

    public decimal? SalarioNeto { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;

    public virtual Nomina IdNominaNavigation { get; set; } = null!;
}
