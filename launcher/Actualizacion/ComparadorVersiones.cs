namespace FUNBIDE.Launcher.Actualizacion;

public static class ComparadorVersiones
{
    /// <summary>
    /// Compara un tag de GitHub Release (ej. "v1.5.0", con o sin la "v" inicial) contra
    /// la versión local. Un tag que no se puede interpretar como versión se trata como
    /// "no hay actualización" en vez de lanzar — nunca debe bloquear el arranque, y
    /// tampoco tiene sentido "actualizar" hacia algo que ni siquiera se puede comparar.
    /// </summary>
    public static bool HayVersionMasNueva(string tagRemoto, Version versionLocal)
    {
        var texto = tagRemoto.Trim();
        if (texto.Length > 0 && (texto[0] == 'v' || texto[0] == 'V'))
        {
            texto = texto[1..];
        }

        return Version.TryParse(texto, out var versionRemota) && versionRemota > versionLocal;
    }
}
