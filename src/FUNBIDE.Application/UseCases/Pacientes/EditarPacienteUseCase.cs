using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Pacientes;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;
using FUNBIDE.Domain.ValueObjects;

namespace FUNBIDE.Application.UseCases.Pacientes;

public interface IEditarPacienteUseCase : IUseCase<EditarPacienteRequest, PacienteDto>
{
}

public sealed class EditarPacienteUseCase(IPacienteRepository pacienteRepository) : IEditarPacienteUseCase
{
    public async Task<PacienteDto> EjecutarAsync(EditarPacienteRequest request, CancellationToken cancellationToken)
    {
        var paciente = await pacienteRepository.ObtenerPorIdAsync(request.PacienteId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Paciente), request.PacienteId);

        var documento = DocumentoIdentidad.Crear(request.Cedula);

        if (documento.Valor != paciente.Documento.Valor)
        {
            var otroPacienteConEsaCedula = await pacienteRepository.ObtenerPorDocumentoAsync(documento.Valor, cancellationToken);
            if (otroPacienteConEsaCedula is not null && otroPacienteConEsaCedula.Id != paciente.Id)
            {
                throw new CedulaEnUsoException(documento.Valor);
            }
        }

        paciente.ActualizarDatos(request.Nombre, request.Apellido, documento, request.Telefono);
        await pacienteRepository.GuardarCambiosAsync(cancellationToken);

        return new PacienteDto(
            paciente.Id, paciente.Nombre, paciente.Apellido, paciente.Documento.Valor, paciente.Telefono, paciente.FotoCedulaPath is not null);
    }
}
