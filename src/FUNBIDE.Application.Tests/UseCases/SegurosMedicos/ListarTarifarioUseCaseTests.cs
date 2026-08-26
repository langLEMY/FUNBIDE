using FUNBIDE.Application.DTOs.SegurosMedicos;
using FUNBIDE.Application.UseCases.SegurosMedicos;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;
using NSubstitute;

namespace FUNBIDE.Application.Tests.UseCases.SegurosMedicos;

public class ListarTarifarioUseCaseTests
{
    [Fact]
    public async Task EjecutarAsync_MapeaLasFilasDelRepositorioADto()
    {
        var seguroId = Guid.NewGuid();
        var tarifarioRepository = Substitute.For<ITarifarioProcedimientoRepository>();
        var fila = new TarifarioProcedimiento(seguroId, PlanAseguradora.Pensionado, "Consulta odontológica general", 140m, 360m, 500m);
        tarifarioRepository
            .ObtenerPorSeguroYPlanAsync(seguroId, PlanAseguradora.Pensionado, Arg.Any<CancellationToken>())
            .Returns(new List<TarifarioProcedimiento> { fila });

        var resultado = await new ListarTarifarioUseCase(tarifarioRepository).EjecutarAsync(
            new ListarTarifarioRequest(seguroId, PlanAseguradora.Pensionado), CancellationToken.None);

        var dto = Assert.Single(resultado);
        Assert.Equal(fila.Id, dto.Id);
        Assert.Equal("Pensionado", dto.Plan);
        Assert.Equal("Consulta odontológica general", dto.Procedimiento);
        Assert.Equal(140m, dto.MontoSeguro);
        Assert.Equal(360m, dto.MontoPaciente);
        Assert.Equal(500m, dto.MontoTotal);
    }
}
