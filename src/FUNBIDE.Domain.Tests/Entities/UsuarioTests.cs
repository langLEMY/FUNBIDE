using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Tests.Entities;

public class UsuarioTests
{
    private static Usuario CrearUsuario(RolUsuario rol, EspecialidadMedica? especialidad = null) =>
        new(Guid.NewGuid(), "Ana Pérez", "ana@funbide.org", "ana.perez", rol, especialidad);

    [Fact]
    public void Constructor_DoctorConEspecialidad_LaAsigna()
    {
        var usuario = CrearUsuario(RolUsuario.Doctor, EspecialidadMedica.Pediatria);

        Assert.Equal(EspecialidadMedica.Pediatria, usuario.Especialidad);
    }

    [Fact]
    public void Constructor_RolNoDoctorConEspecialidad_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CrearUsuario(RolUsuario.Admin, EspecialidadMedica.Cardiologia));
    }

    [Fact]
    public void AsignarEspecialidad_UsuarioNoDoctor_LanzaArgumentException()
    {
        var usuario = CrearUsuario(RolUsuario.Fondos);

        Assert.Throws<ArgumentException>(() => usuario.AsignarEspecialidad(EspecialidadMedica.Ginecologia));
    }

    [Fact]
    public void CambiarRol_AlejandoseDeDoctor_LimpiaEspecialidad()
    {
        var usuario = CrearUsuario(RolUsuario.Doctor, EspecialidadMedica.Odontologia);

        usuario.CambiarRol(RolUsuario.Admin);

        Assert.Null(usuario.Especialidad);
    }

    [Fact]
    public void CambiarRol_EntreDoctores_ConservaEspecialidad()
    {
        var usuario = CrearUsuario(RolUsuario.Doctor, EspecialidadMedica.Oftalmologia);

        usuario.CambiarRol(RolUsuario.Doctor);

        Assert.Equal(EspecialidadMedica.Oftalmologia, usuario.Especialidad);
    }
}
