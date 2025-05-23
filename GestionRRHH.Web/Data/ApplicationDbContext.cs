using System;
using System.Collections.Generic;
using GestionRRHH.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace GestionRRHH.Web.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Asistencia> Asistencias { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<DetalleNomina> DetalleNominas { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<Liquidacione> Liquidaciones { get; set; }

    public virtual DbSet<Nomina> Nominas { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<Prestacione> Prestaciones { get; set; }

    public virtual DbSet<Puesto> Puestos { get; set; }

    public virtual DbSet<SuspensionesIgss> SuspensionesIgsses { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Vacacione> Vacaciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Asistencia>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.IdEmpleado, e.Fecha }, "UK_Empleado_Fecha").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.HoraEntrada).HasColumnType("time");
            entity.Property(e => e.HoraSalida).HasColumnType("time");
            entity.Property(e => e.HorasExtra)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.HorasTrabajadas)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.IdEmpleado)
                .HasColumnType("int(11)")
                .HasColumnName("ID_Empleado");
            entity.Property(e => e.Observaciones).HasColumnType("text");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Asistencia)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("asistencias_ibfk_1");
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.ResponsableId, "FK_Departamentos_Responsable");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.ResponsableId)
                .HasColumnType("int(11)")
                .HasColumnName("ResponsableID");

            entity.HasOne(d => d.Responsable).WithMany(p => p.Departamentos)
                .HasForeignKey(d => d.ResponsableId)
                .HasConstraintName("FK_Departamentos_Responsable");
        });

        modelBuilder.Entity<DetalleNomina>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("DetalleNomina");

            entity.HasIndex(e => e.IdEmpleado, "ID_Empleado");

            entity.HasIndex(e => new { e.IdNomina, e.IdEmpleado }, "UK_Nomina_Empleado").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.Aguinaldo)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.BonificacionIncentivo)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'250.00'");
            entity.Property(e => e.Bonificaciones)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.Bono14)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.Comisiones)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.DescuentosJudiciales)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.DiasLaborados)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.HorasExtra)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.IdEmpleado)
                .HasColumnType("int(11)")
                .HasColumnName("ID_Empleado");
            entity.Property(e => e.IdNomina)
                .HasColumnType("int(11)")
                .HasColumnName("ID_Nomina");
            entity.Property(e => e.Igss)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("IGSS");
            entity.Property(e => e.Isr)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("ISR");
            entity.Property(e => e.MontoHorasExtra)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.OtrosDescuentos)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.OtrosIngresos)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.SalarioBase).HasPrecision(10, 2);
            entity.Property(e => e.SalarioNeto)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.TotalDescuentos)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.TotalIngresos)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.DetalleNominas)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("detallenomina_ibfk_2");

            entity.HasOne(d => d.IdNominaNavigation).WithMany(p => p.DetalleNominas)
                .HasForeignKey(d => d.IdNomina)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("detallenomina_ibfk_1");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.Dpi, "DPI").IsUnique();

            entity.HasIndex(e => e.DepartamentoId, "DepartamentoID");

            entity.HasIndex(e => e.PuestoId, "PuestoID");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.DepartamentoId)
                .HasColumnType("int(11)")
                .HasColumnName("DepartamentoID");
            entity.Property(e => e.Direccion).HasColumnType("text");
            entity.Property(e => e.Dpi)
                .HasMaxLength(20)
                .HasColumnName("DPI");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'activo'")
                .HasColumnType("enum('activo','inactivo')");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.NombreCompleto).HasMaxLength(150);
            entity.Property(e => e.PuestoId)
                .HasColumnType("int(11)")
                .HasColumnName("PuestoID");
            entity.Property(e => e.SalarioBase).HasPrecision(10, 2);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.TipoNomina).HasColumnType("enum('mensual','quincenal','semanal')");

            entity.HasOne(d => d.Departamento).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.DepartamentoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("empleados_ibfk_1");

            entity.HasOne(d => d.Puesto).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.PuestoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("empleados_ibfk_2");
        });

        modelBuilder.Entity<Liquidacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.IdEmpleado, "ID_Empleado");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.AguinaldoProporcional)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.AntiguedadAnios).HasColumnType("int(11)");
            entity.Property(e => e.Bono14Proporcional)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'calculada'")
                .HasColumnType("enum('calculada','pagada')");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.IdEmpleado)
                .HasColumnType("int(11)")
                .HasColumnName("ID_Empleado");
            entity.Property(e => e.Indemnizacion)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.PagoVacaciones)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.TotalLiquidacion).HasPrecision(10, 2);
            entity.Property(e => e.VacacionesPendientes)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Liquidaciones)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("liquidaciones_ibfk_1");
        });

        modelBuilder.Entity<Nomina>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'en_proceso'")
                .HasColumnType("enum('en_proceso','finalizada','pagada')");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("curdate()");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.Tipo).HasColumnType("enum('mensual','quincenal','semanal')");
            entity.Property(e => e.UsuarioCreacion).HasMaxLength(50);
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.IdEmpleado, "ID_Empleado");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.Descuento)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.Dias).HasColumnType("int(11)");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'solicitado'")
                .HasColumnType("enum('solicitado','aprobado','rechazado')");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.IdEmpleado)
                .HasColumnType("int(11)")
                .HasColumnName("ID_Empleado");
            entity.Property(e => e.Motivo).HasColumnType("text");
            entity.Property(e => e.Tipo).HasColumnType("enum('con_goce','sin_goce')");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Permisos)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("permisos_ibfk_1");
        });

        modelBuilder.Entity<Prestacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.IdEmpleado, "ID_Empleado");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.Comentarios).HasColumnType("text");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'pendiente'")
                .HasColumnType("enum('pendiente','pagado')");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.IdEmpleado)
                .HasColumnType("int(11)")
                .HasColumnName("ID_Empleado");
            entity.Property(e => e.Monto).HasPrecision(10, 2);
            entity.Property(e => e.Tipo).HasColumnType("enum('vacaciones','aguinaldo','bono14','indemnizacion')");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Prestaciones)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("prestaciones_ibfk_1");
        });

        modelBuilder.Entity<Puesto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.NivelJerarquico).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.SalarioMaximo).HasPrecision(10, 2);
            entity.Property(e => e.SalarioMinimo).HasPrecision(10, 2);
        });

        modelBuilder.Entity<SuspensionesIgss>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("SuspensionesIGSS");

            entity.HasIndex(e => e.IdEmpleado, "ID_Empleado");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.DiasPagadosIgss)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)")
                .HasColumnName("DiasPagadosIGSS");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'activa'")
                .HasColumnType("enum('activa','finalizada')");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.IdEmpleado)
                .HasColumnType("int(11)")
                .HasColumnName("ID_Empleado");
            entity.Property(e => e.MontoSubsidio)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.Observaciones).HasColumnType("text");
            entity.Property(e => e.Tipo).HasColumnType("enum('enfermedad_comun','accidente_laboral')");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.SuspensionesIgsses)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("suspensionesigss_ibfk_1");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.IdEmpleado, "ID_Empleado");

            entity.HasIndex(e => e.NombreUsuario, "NombreUsuario").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.Contraseña).HasMaxLength(255);
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'activo'")
                .HasColumnType("enum('activo','inactivo')");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.IdEmpleado)
                .HasColumnType("int(11)")
                .HasColumnName("ID_Empleado");
            entity.Property(e => e.NombreUsuario).HasMaxLength(50);
            entity.Property(e => e.Rol).HasColumnType("enum('admin','rrhh','supervisor')");
            entity.Property(e => e.UltimoAcceso).HasColumnType("datetime");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("usuarios_ibfk_1");
        });

        modelBuilder.Entity<Vacacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.IdEmpleado, "ID_Empleado");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.DiasTomados).HasColumnType("int(11)");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'solicitada'")
                .HasColumnType("enum('solicitada','aprobada','pagada','rechazada')");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.IdEmpleado)
                .HasColumnType("int(11)")
                .HasColumnName("ID_Empleado");
            entity.Property(e => e.PagoTotal).HasPrecision(10, 2);
            entity.Property(e => e.PromedioBonosExtras)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Vacaciones)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vacaciones_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
