using FUNBIDE.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Sirve archivos del almacenamiento local (modo Auth:Provider=Local) validando la firma
/// HMAC generada por <c>LocalDiskStorageService</c> — el equivalente local a pedir una URL
/// firmada de Supabase Storage. Deliberadamente anónimo: la "autenticación" viaja en la
/// propia URL (ruta/expira/firma), no en un header, para poder consumirse desde un
/// <c>&lt;img src&gt;</c> directo. En modo Supabase este endpoint nunca se usa: el frontend
/// nunca genera una URL con este formato.
/// </summary>
[ApiController]
[Route("api/archivos-locales")]
public sealed class ArchivoLocalController(IArchivoLocalService archivoLocal) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> AbrirAsync(
        [FromQuery] string ruta, [FromQuery] long expira, [FromQuery] string firma, CancellationToken cancellationToken)
    {
        var resultado = await archivoLocal.AbrirSiFirmaValidaAsync(ruta, expira, firma, cancellationToken);
        return resultado is null ? NotFound() : File(resultado.Contenido, resultado.ContentType);
    }
}
