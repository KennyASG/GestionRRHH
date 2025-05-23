using System;
using System.Collections.Generic;

namespace GestionRRHH.Web.Models.Entities;

public partial class Puesto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string NivelJerarquico { get; set; } = null!;

    public decimal SalarioMinimo { get; set; }

    public decimal SalarioMaximo { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
