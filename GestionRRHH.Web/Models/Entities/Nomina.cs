using System;
using System.Collections.Generic;

namespace GestionRRHH.Web.Models.Entities;

public partial class Nomina
{
    public int Id { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public string Tipo { get; set; } = null!;

    public string? Estado { get; set; }

    public DateOnly? FechaCreacion { get; set; }

    public string UsuarioCreacion { get; set; } = null!;

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<DetalleNomina> DetalleNominas { get; set; } = new List<DetalleNomina>();
}
