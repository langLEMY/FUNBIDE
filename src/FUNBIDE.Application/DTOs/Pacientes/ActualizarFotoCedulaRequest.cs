namespace FUNBIDE.Application.DTOs.Pacientes;

public sealed record ActualizarFotoCedulaRequest(Guid PacienteId, Stream Contenido, string ContentType);
