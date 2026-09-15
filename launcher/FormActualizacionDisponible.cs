using FUNBIDE.Launcher.Actualizacion;

namespace FUNBIDE.Launcher;

/// <summary>
/// Aviso de actualización disponible, como una notificación flotante en la esquina —
/// nunca un ShowDialog: se muestra con .Show() (no modal) para que se pueda seguir
/// usando FUNBIDE con normalidad (una consulta, un cobro en curso, etc.) mientras el
/// aviso queda ahí, ignorable, hasta que el usuario decida actualizar o cerrarlo.
/// </summary>
public sealed class FormActualizacionDisponible : Form
{
    private readonly ServicioActualizacion _servicioActualizacion;
    private readonly InfoActualizacion _info;

    private readonly Label _etiqueta = new() { Dock = DockStyle.Top, Height = 50, TextAlign = ContentAlignment.MiddleCenter };
    private readonly Button _botonActualizar = new() { Text = "Actualizar ahora", Dock = DockStyle.Bottom, Height = 32 };
    private readonly Button _botonMasTarde = new() { Text = "Más tarde", Dock = DockStyle.Bottom, Height = 28 };
    private readonly ProgressBar _barraProgreso = new() { Dock = DockStyle.Bottom, Style = ProgressBarStyle.Marquee, Visible = false };

    public FormActualizacionDisponible(ServicioActualizacion servicioActualizacion, InfoActualizacion info)
    {
        _servicioActualizacion = servicioActualizacion;
        _info = info;

        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        Text = "FUNBIDE";
        ClientSize = new Size(340, 130);
        MaximizeBox = false;
        MinimizeBox = false;

        _etiqueta.Text = $"Hay una versión nueva disponible ({_info.VersionTag}).";
        _botonActualizar.Click += async (_, _) => await ActualizarAsync();
        _botonMasTarde.Click += (_, _) => Close();

        // Orden de Controls.Add importa con Dock: el último agregado queda más "afuera".
        Controls.Add(_etiqueta);
        Controls.Add(_barraProgreso);
        Controls.Add(_botonMasTarde);
        Controls.Add(_botonActualizar);

        Load += (_, _) =>
        {
            var area = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1024, 768);
            Location = new Point(area.Right - Width - 16, area.Bottom - Height - 16);
        };
    }

    private async Task ActualizarAsync()
    {
        _botonActualizar.Enabled = false;
        _botonMasTarde.Enabled = false;
        _barraProgreso.Visible = true;
        _etiqueta.Text = "Descargando actualización…";

        try
        {
            using var cancelacion = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var rutaInstalador = await _servicioActualizacion.DescargarInstaladorAsync(_info, cancelacion.Token);

            // Sin /VERYSILENT ni /SUPPRESSMSGBOXES a propósito: el .exe todavía no tiene
            // firma de código, así que descargarlo y correrlo en silencio sería
            // indistinguible del patrón de un malware. El usuario tiene que ver y
            // confirmar el instalador de Inno Setup como cualquier otro .exe bajado de
            // internet — eso cambia recién cuando haya certificado de firma de código.
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = rutaInstalador,
                UseShellExecute = true,
            });

            // Cierra todo el launcher (incluye FormPrincipal, que en su propio
            // FormClosing mata procesoBackend si fue este launcher el que lo arrancó) para
            // que el instalador pueda reemplazar los archivos sin nada bloqueándolos.
            Application.Exit();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "No se pudo descargar o verificar la actualización:\n\n" + ex.Message +
                "\n\nFUNBIDE sigue funcionando con la versión actual — podés intentarlo de nuevo más tarde.",
                "FUNBIDE — actualización",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            _botonActualizar.Enabled = true;
            _botonMasTarde.Enabled = true;
            _barraProgreso.Visible = false;
            _etiqueta.Text = $"Hay una versión nueva disponible ({_info.VersionTag}).";
        }
    }
}
