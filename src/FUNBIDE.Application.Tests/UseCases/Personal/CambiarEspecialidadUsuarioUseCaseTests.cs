using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Application.UseCases.Personal;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.Personal;

public class CambiarEspecialidadUsuarioUseCaseTests
{
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IAuditoriaLogService _auditoriaLogService = Substitute.For<IAuditoriaLogService>();

    public CambiarEspecialidadUsuarioUseCaseTests()
    {
        _currentUser.UsuarioId.Returns(Guid.NewGuid());
    }

    private CambiarEspecialidadUsuarioUseCase CrearCasoDeUso() =>
        new(_usuarioRepository, _currentUser, _auditoriaLogService);

    [Fact]
    public async Task EjecutarAsync_UsuarioEsDoctor_AsignaLaEspecialidadYAuditaElCambio()
    {
        var doctor = new Usuario(Guid.NewGuid(), "Doctor De Prueba", "doctor@funbide.local", "doctor.prueba", RolUsuario.Doctor);
        _usuarioRepository.ObtenerPorIdAsync(doctor.Id, Arg.Any<CancellationToken>()).Returns(doctor);

        var resultado = await CrearCasoDeUso().EjecutarAsync(
            new CambiarEspecialidadRequest(doctor.Id, EspecialidadMedica.Pediatria), CancellationToken.None);

        Assert.Equal(EspecialidadMedica.Pediatria, resultado.Especialidad);
        Assert.Equal(EspecialidadMedica.Pediatria, doctor.Especialidad);
        await _usuarioRepository.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
        await _auditoriaLogService.Received(1).RegistrarEventoAsync(
            "personal.cambiar-especialidad", Arg.Any<string>(), Arg.Any<object>(), Arg.Any<Guid?>(), 200, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_UsuarioNoEsDoctor_RechazaSinGuardarNiAuditar()
    {
        var admin = new Usuario(Guid.NewGuid(), "Admin De Prueba", "admin@funbide.local", "admin.prueba", RolUsuario.Admin);
        _usuarioRepository.ObtenerPorIdAsync(admin.Id, Arg.Any<CancellationToken>()).Returns(admin);

        await Assert.ThrowsAsync<OperacionNoPermitidaException>(() => CrearCasoDeUso().EjecutarAsync(
            new CambiarEspecialidadRequest(admin.Id, EspecialidadMedica.Pediatria), CancellationToken.None));

        await _usuarioRepository.DidNotReceive().GuardarCambiosAsync(Arg.Any<CancellationToken>());
        await _auditoriaLogService.DidNotReceive().RegistrarEventoAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<object>(), Arg.Any<Guid?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_UsuarioNoExiste_LanzaRecursoNoEncontrado()
    {
        _usuarioRepository.ObtenerPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => CrearCasoDeUso().EjecutarAsync(
            new CambiarEspecialidadRequest(Guid.NewGuid(), EspecialidadMedica.Pediatria), CancellationToken.None));
    }
}
