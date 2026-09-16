namespace FUNBIDE.Application.DTOs.Cobros;

public sealed record RegistrarCobroRequest(
    Guid PacienteId,
    Guid? CitaId,
    string Concepto,
    decimal MontoTotal,
    /// <summary>
    /// Cómo se paga lo que queda a cargo del paciente — puede traer 0, 1 o varias líneas
    /// (ej. una parte con tarjeta y otra en efectivo). Reemplaza el viejo par
    /// (MetodoPago, MontoPagado) de un solo método.
    /// </summary>
    IReadOnlyList<PagoDto> Pagos,
    Guid? SeguroMedicoId,
    string? CodigoAutorizacion,
    /// <summary>
    /// Si se informa, <see cref="MontoTotal"/> y el copago se recalculan del tarifario
    /// (ver <c>TarifarioProcedimiento</c>) en el servidor — lo que mande el cliente en
    /// <see cref="MontoTotal"/> se ignora, igual que ya pasa con el % de cobertura.
    /// </summary>
    Guid? TarifarioProcedimientoId = null,
    /// <summary>Doctor con el que se atiende el paciente — SupabaseUserId, igual que <c>Cita.DoctorId</c>.</summary>
    Guid? DoctorId = null,
    /// <summary>
    /// Generada por el cliente (ej. un GUID nuevo por cada intento de submit del
    /// formulario), no por el servidor. Si se repite, RegistrarCobroUseCase devuelve el
    /// cobro ya creado en vez de duplicarlo — protege contra un doble-click en "Registrar"
    /// o un reintento automático de red tras un timeout que en realidad sí procesó.
    /// Opcional: un cliente que no la manda simplemente no queda protegido.
    /// </summary>
    string? ClaveIdempotencia = null);
