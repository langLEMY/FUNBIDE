namespace FUNBIDE.Application.Exceptions;

public sealed class RecursoNoEncontradoException : Exception
{
    public RecursoNoEncontradoException(string recurso, Guid id)
        : base($"No se encontró '{recurso}' con id '{id}'.")
    {
    }
}
