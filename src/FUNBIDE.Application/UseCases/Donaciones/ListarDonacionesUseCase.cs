using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Donaciones;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Donaciones;

public interface IListarDonacionesUseCase : IUseCase<ListarDonacionesRequest, IReadOnlyList<DonacionDto>>
{
}

public sealed class ListarDonacionesUseCase(IDonacionRepository donacionRepository) : IListarDonacionesUseCase
{
    public async Task<IReadOnlyList<DonacionDto>> EjecutarAsync(ListarDonacionesRequest request, CancellationToken cancellationToken)
    {
        var (desde, hasta) = RangoFechasHelper.RecortarASpanMaximo(request.Desde, request.Hasta);
        var donaciones = await donacionRepository.ObtenerPorRangoAsync(desde, hasta, cancellationToken);
        return donaciones
            .Select(d => new DonacionDto(d.Id, d.DonanteNombre, d.DonanteContacto, d.Monto, d.Concepto, d.RegistradoEn))
            .ToList();
    }
}
