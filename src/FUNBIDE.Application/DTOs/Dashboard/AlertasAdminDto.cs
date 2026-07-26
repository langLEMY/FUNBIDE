namespace FUNBIDE.Application.DTOs.Dashboard;

public sealed record ItemStockBajoDto(Guid Id, string Codigo, string Nombre, int StockActual, int StockMinimo);

public sealed record PacienteConDeudaDto(Guid PacienteId, string PacienteNombre, decimal MontoAdeudado);

public sealed record AlertasAdminDto(
    IReadOnlyList<ItemStockBajoDto> StockBajo, IReadOnlyList<PacienteConDeudaDto> PacientesConDeuda);
