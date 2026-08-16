using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.SegurosMedicos;
using FUNBIDE.Application.UseCases.SegurosMedicos;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.SegurosMedicos;

public class ImportarTarifarioUseCaseTests
{
    private readonly IExcelLectorService _excelLector = Substitute.For<IExcelLectorService>();
    private readonly ITarifarioProcedimientoRepository _tarifarioRepository = Substitute.For<ITarifarioProcedimientoRepository>();

    private ImportarTarifarioUseCase CrearCasoDeUso() => new(_excelLector, _tarifarioRepository);

    private static Dictionary<string, string?> Fila(string procedimiento, string seguro, string paciente, string? total = null) =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Procedimiento"] = procedimiento,
            ["Seguro"] = seguro,
            ["Paciente"] = paciente,
            ["Total"] = total,
        };

    [Fact]
    public async Task EjecutarAsync_ProcedimientoNuevo_LoCrea()
    {
        _tarifarioRepository
            .ObtenerParaImportarAsync(Arg.Any<Guid>(), Arg.Any<PlanSenasa>(), Arg.Any<CancellationToken>())
            .Returns(new List<TarifarioProcedimiento>());
        _excelLector.LeerFilas(Arg.Any<Stream>()).Returns(
        [
            Fila("Consulta odontológica general", "690", "100", "790"),
        ]);

        var seguroId = Guid.NewGuid();
        var resultado = await CrearCasoDeUso().EjecutarAsync(
            new ImportarTarifarioRequest(seguroId, PlanSenasa.Contributivo, Stream.Null), CancellationToken.None);

        Assert.Equal(1, resultado.Creados);
        Assert.Equal(0, resultado.Actualizados);
        await _tarifarioRepository.Received(1).AgregarAsync(
            Arg.Is<TarifarioProcedimiento>(t =>
                t.SeguroMedicoId == seguroId && t.Procedimiento == "Consulta odontológica general" &&
                t.MontoSeguro == 690m && t.MontoPaciente == 100m && t.MontoTotal == 790m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_ProcedimientoYaExiste_ActualizaMontosEnVezDeDuplicar()
    {
        var seguroId = Guid.NewGuid();
        var existente = new TarifarioProcedimiento(
            seguroId, PlanSenasa.Contributivo, "Consulta odontológica general", 100m, 50m, 150m);
        _tarifarioRepository
            .ObtenerParaImportarAsync(seguroId, PlanSenasa.Contributivo, Arg.Any<CancellationToken>())
            .Returns(new List<TarifarioProcedimiento> { existente });
        _excelLector.LeerFilas(Arg.Any<Stream>()).Returns(
        [
            Fila("Consulta odontológica general", "690", "100", "790"),
        ]);

        var resultado = await CrearCasoDeUso().EjecutarAsync(
            new ImportarTarifarioRequest(seguroId, PlanSenasa.Contributivo, Stream.Null), CancellationToken.None);

        Assert.Equal(0, resultado.Creados);
        Assert.Equal(1, resultado.Actualizados);
        Assert.Equal(690m, existente.MontoSeguro);
        Assert.Equal(790m, existente.MontoTotal);
        await _tarifarioRepository.DidNotReceive().AgregarAsync(Arg.Any<TarifarioProcedimiento>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_TotalEnBlanco_LoDerivaDeSeguroMasPaciente()
    {
        _tarifarioRepository
            .ObtenerParaImportarAsync(Arg.Any<Guid>(), Arg.Any<PlanSenasa>(), Arg.Any<CancellationToken>())
            .Returns(new List<TarifarioProcedimiento>());
        _excelLector.LeerFilas(Arg.Any<Stream>()).Returns(
        [
            Fila("Obturación clase I", "250", "700", total: null),
        ]);

        await CrearCasoDeUso().EjecutarAsync(
            new ImportarTarifarioRequest(Guid.NewGuid(), PlanSenasa.Contributivo, Stream.Null), CancellationToken.None);

        await _tarifarioRepository.Received(1).AgregarAsync(
            Arg.Is<TarifarioProcedimiento>(t => t.MontoTotal == 950m), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_FilaSinNombreDeProcedimiento_LaOmite()
    {
        _tarifarioRepository
            .ObtenerParaImportarAsync(Arg.Any<Guid>(), Arg.Any<PlanSenasa>(), Arg.Any<CancellationToken>())
            .Returns(new List<TarifarioProcedimiento>());
        _excelLector.LeerFilas(Arg.Any<Stream>()).Returns(
        [
            Fila("", "690", "100", "790"),
        ]);

        var resultado = await CrearCasoDeUso().EjecutarAsync(
            new ImportarTarifarioRequest(Guid.NewGuid(), PlanSenasa.Contributivo, Stream.Null), CancellationToken.None);

        Assert.Equal(0, resultado.Creados);
        Assert.Equal(1, resultado.Omitidos);
    }
}
