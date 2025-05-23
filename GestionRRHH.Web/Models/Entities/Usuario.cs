using System;
using System.Collections.Generic;

namespace GestionRRHH.Web.Models.Entities;

public partial class Usuario
{
    public int Id { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string Contraseña { get; set; } = null!;

    public int IdEmpleado { get; set; }

    public string Rol { get; set; } = null!;

    public string? Estado { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
}
