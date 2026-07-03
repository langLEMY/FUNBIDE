using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Pacientes;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;
using FUNBIDE.Domain.ValueObjects;

namespace FUNBIDE.Application.UseCases.Pacientes;

public interface ICrearPacienteUseCase : IUseCase<CrearPacienteRequest, PacienteDto>
{
}

public sealed class CrearPacienteUseCase(IPacienteRepository pacienteRepository) : ICrearPacienteUseCase
{
    public async Task<PacienteDto> EjecutarAsync(CrearPacienteRequest request, CancellationToken cancellationToken)
    {
        var documento = DocumentoIdentidad.Crear(request.Cedula);

        if (await pacienteRepository.ObtenerPorDocumentoAsync(documento.Valor, cancellationToken) is not null)
        {
            throw new CedulaEnUsoException(documento.Valor);
        }

        var paciente = new Paciente(request.Nombre, request.Apellido, documento, request.Telefono);

        await pacienteRepository.AgregarAsync(paciente, cancellationToken);
        await pacienteRepository.GuardarCambiosAsync(cancellationToken);

        return new PacienteDto(paciente.Id, paciente.Nombre, paciente.Apellido, paciente.Documento.Valor, paciente.Telefono, false);
    }
}
