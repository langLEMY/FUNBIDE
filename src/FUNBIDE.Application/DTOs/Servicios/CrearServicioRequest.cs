using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Servicios;

public sealed record CrearServicioRequest(
    string Codigo, string Nombre, decimal Precio1, decimal Precio2, decimal Precio3, EspecialidadMedica? Especialidad);
