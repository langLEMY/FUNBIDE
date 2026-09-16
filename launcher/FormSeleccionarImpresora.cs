using System.Drawing.Printing;

namespace FUNBIDE.Launcher;

/// <summary>
/// Diálogo modal simple para elegir a qué impresora instalada en Windows mandar el
/// comprobante antes de imprimirlo (antes se mandaba siempre a la predeterminada del
/// sistema sin preguntar). Recuerda la última impresora elegida en
/// %LocalAppData%\FUNBIDE\ultima-impresora.txt y la deja preseleccionada la próxima vez.
/// </summary>
public sealed class FormSeleccionarImpresora : Form
{
    private static readonly string RutaPreferencia = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FUNBIDE", "ultima-impresora.txt");

    private readonly ComboBox _combo = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _etiqueta = new() { Dock = DockStyle.Top, Height = 28, Text = "¿A qué impresora imprimimos el comprobante?" };
    private readonly Button _botonImprimir = new() { Text = "Imprimir", DialogResult = DialogResult.OK };
    private readonly Button _botonCancelar = new() { Text = "Cancelar", DialogResult = DialogResult.Cancel };

    public string? ImpresoraSeleccionada { get; private set; }

    public FormSeleccionarImpresora()
    {
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MaximizeBox = false;
        MinimizeBox = false;
        Text = "FUNBIDE — imprimir";
        ClientSize = new Size(360, 120);
        AcceptButton = _botonImprimir;
        CancelButton = _botonCancelar;

        _combo.Width = 320;
        _combo.Location = new Point(16, 40);
        foreach (var nombre in PrinterSettings.InstalledPrinters)
        {
            _combo.Items.Add(nombre);
        }

        var predeterminada = new PrinterSettings().PrinterName;
        var ultimaUsada = LeerUltimaImpresora();
        var aPreseleccionar = ultimaUsada is not null && _combo.Items.Contains(ultimaUsada) ? ultimaUsada : predeterminada;
        if (_combo.Items.Contains(aPreseleccionar))
        {
            _combo.SelectedItem = aPreseleccionar;
        }
        else if (_combo.Items.Count > 0)
        {
            _combo.SelectedIndex = 0;
        }

        _botonImprimir.Location = new Point(184, 80);
        _botonImprimir.Size = new Size(80, 28);
        _botonCancelar.Location = new Point(272, 80);
        _botonCancelar.Size = new Size(80, 28);

        _botonImprimir.Click += (_, _) =>
        {
            ImpresoraSeleccionada = _combo.SelectedItem as string;
            if (ImpresoraSeleccionada is not null)
            {
                GuardarUltimaImpresora(ImpresoraSeleccionada);
            }
        };

        Controls.Add(_combo);
        Controls.Add(_etiqueta);
        Controls.Add(_botonImprimir);
        Controls.Add(_botonCancelar);
    }

    private static string? LeerUltimaImpresora()
    {
        try
        {
            return File.Exists(RutaPreferencia) ? File.ReadAllText(RutaPreferencia).Trim() : null;
        }
        catch
        {
            return null;
        }
    }

    private static void GuardarUltimaImpresora(string nombre)
    {
        try
        {
            var carpeta = Path.GetDirectoryName(RutaPreferencia);
            if (carpeta is not null)
            {
                Directory.CreateDirectory(carpeta);
            }

            File.WriteAllText(RutaPreferencia, nombre);
        }
        catch
        {
            // No es crítico: si falla, simplemente no se recuerda la próxima vez.
        }
    }
}
