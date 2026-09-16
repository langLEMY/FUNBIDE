using FUNBIDE.Application.DTOs.Pacientes;
using FUNBIDE.Application.Validators;

namespace FUNBIDE.Application.Tests.Validators;

public class CrearPacienteRequestValidatorTests
{
    private readonly CrearPacienteRequestValidator _validator = new();

    [Fact]
    public void Validate_NombreVacio_DevuelveMensajeEnEspanolDirigidoAlUsuario()
    {
        var request = new CrearPacienteRequest("", "Pérez", "12345", null, null, null);

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "El nombre es obligatorio.");
    }

    [Fact]
    public void Validate_CedulaMuyCorta_DevuelveMensajeConElRangoPermitido()
    {
        var request = new CrearPacienteRequest("Ana", "Pérez", "12", null, null, null);

        var resultado = _validator.Validate(request);

        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "La cédula debe tener entre 5 y 20 caracteres.");
    }

    [Fact]
    public void Validate_DatosValidos_NoDevuelveErrores()
    {
        var request = new CrearPacienteRequest("Ana", "Pérez", "00112345678", "8091234567", 30, null);

        var resultado = _validator.Validate(request);

        Assert.True(resultado.IsValid);
    }
}
