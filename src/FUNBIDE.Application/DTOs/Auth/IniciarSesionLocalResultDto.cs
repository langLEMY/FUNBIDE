namespace FUNBIDE.Application.DTOs.Auth;

public sealed record IniciarSesionLocalResultDto(string AccessToken, DateTimeOffset ExpiraEn);
