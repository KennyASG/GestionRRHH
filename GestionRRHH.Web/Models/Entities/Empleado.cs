using System;
using System.Collections.Generic;

namespace GestionRRHH.Web.Models.Entities;

public partial class Empleado
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string Dpi { get; set; } = null!;

    public DateOnly FechaNacimiento { get; set; }

    public string Direccion { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string? Email { get; set; }

    public DateOnly FechaContratacion { get; set; }

    public int DepartamentoId { get; set; }

    public int PuestoId { get; set; }

    public decimal SalarioBase { get; set; }

    public string TipoNomina { get; set; } = null!;

    public string? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<Asistencia> Asistencia { get; set; } = new List<Asistencia>();

    public virtual Departamento Departamento { get; set; } = null!;

    public virtual ICollection<Departamento> Departamentos { get; set; } = new List<Departamento>();

    public virtual ICollection<DetalleNomina> DetalleNominas { get; set; } = new List<DetalleNomina>();

    public virtual ICollection<Liquidacione> Liquidaciones { get; set; } = new List<Liquidacione>();

    public virtual ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();

    public virtual ICollection<Prestacione> Prestaciones { get; set; } = new List<Prestacione>();

    public virtual Puesto Puesto { get; set; } = null!;

    public virtual ICollection<SuspensionesIgss> SuspensionesIgsses { get; set; } = new List<SuspensionesIgss>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    public virtual ICollection<Vacacione> Vacaciones { get; set; } = new List<Vacacione>();
}
