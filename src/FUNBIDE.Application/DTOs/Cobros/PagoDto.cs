using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Cobros;

/// <summary>Una línea del desglose de pago de un cobro (ver PagoRecibido). Se usa tanto para pedir un cobro nuevo como para mostrar uno ya registrado.</summary>
public sealed record PagoDto(MetodoPago Metodo, decimal Monto);
