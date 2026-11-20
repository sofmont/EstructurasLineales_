using System.ComponentModel;
using EstructurasLineales.Core.Navegacion;

namespace EstructurasLineales.App.Controles;

/// <summary>Escenario 1: historial de navegación web con dos pilas (Atrás / Adelante).</summary>
[DesignerCategory("Code")]
public class NavegadorControl : UserControl
{
    private static readonly string[] SitiosRapidos =
        { "google.com", "youtube.com", "github.com", "wikipedia.org", "microsoft.com", "stackoverflow.com" };

    private readonly HistorialNavegador _historial = new();

    private readonly Button _btnAtras = Ui.Boton("◀  Atrás", Ui.Primario, 110);
    private readonly Button _btnAdelante = Ui.Boton("Adelante  ▶", Ui.Primario, 110);
    private readonly Button _btnIr = Ui.Boton("Ir", Ui.Exito, 70);
    private readonly Button _btnReiniciar = Ui.Boton("Reiniciar", Ui.Neutro, 100);
    private readonly TextBox _txtUrl = new()
    {
        Dock = DockStyle.Fill,
        Font = new Font("Segoe UI", 11F),
        Margin = new Padding(4, 7, 4, 4),
        PlaceholderText = "Escribe una dirección (ej. github.com) y presiona Enter"
    };
    private readonly Label _lblPagina = new()
    {
        Dock = DockStyle.Top,
        Height = 86,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Segoe UI", 16F, FontStyle.Bold),
        BackColor = Color.White,
        ForeColor = Ui.Oscuro,
        BorderStyle = BorderStyle.FixedSingle
    };
    private readonly VisorPila _visorAtras = new() { ColorTope = Ui.Primario };
    private readonly VisorPila _visorAdelante = new() { ColorTope = Ui.Advertencia };
    private readonly ListBox _lstBitacora = Ui.Bitacora();

    public NavegadorControl()
    {
        BackColor = Ui.Fondo;

        // --- Barra de navegación ---
        var barra = new TableLayoutPanel { Dock = DockStyle.Top, Height = 44, ColumnCount = 5, RowCount = 1 };
        barra.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));
        barra.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));
        barra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        barra.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78));
        barra.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108));
        barra.Controls.Add(_btnAtras, 0, 0);
        barra.Controls.Add(_btnAdelante, 1, 0);
        barra.Controls.Add(_txtUrl, 2, 0);
        barra.Controls.Add(_btnIr, 3, 0);
        barra.Controls.Add(_btnReiniciar, 4, 0);

        // --- Accesos rápidos ---
        var rapidos = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 32, Padding = new Padding(2, 4, 0, 0) };
        rapidos.Controls.Add(new Label { Text = "Sitios de ejemplo:", AutoSize = true, Margin = new Padding(4, 3, 6, 0) });
        foreach (string sitio in SitiosRapidos)
        {
            var enlace = new LinkLabel { Text = sitio, AutoSize = true, Margin = new Padding(4, 3, 8, 0) };
            enlace.LinkClicked += (_, _) => Navegar(sitio);
            rapidos.Controls.Add(enlace);
        }

        // --- Cuerpo: dos pilas + bitácora ---
        var cuerpo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(0, 8, 0, 0) };
        cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        cuerpo.Controls.Add(Ui.Grupo("Pila ATRÁS  (páginas anteriores)", _visorAtras), 0, 0);
        cuerpo.Controls.Add(Ui.Grupo("Pila ADELANTE  (páginas siguientes)", _visorAdelante), 1, 0);
        cuerpo.Controls.Add(Ui.Grupo("Bitácora de operaciones PUSH / POP", _lstBitacora), 2, 0);

        // El último agregado se acopla primero (queda más arriba).
        Controls.Add(cuerpo);
        Controls.Add(_lblPagina);
        Controls.Add(rapidos);
        Controls.Add(barra);

        // --- Eventos ---
        _btnIr.Click += (_, _) => Navegar(_txtUrl.Text);
        _btnAtras.Click += (_, _) => Retroceder();
        _btnAdelante.Click += (_, _) => Avanzar();
        _btnReiniciar.Click += (_, _) =>
        {
            _historial.Reiniciar();
            _lstBitacora.Items.Clear();
            Refrescar();
        };
        _txtUrl.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Navegar(_txtUrl.Text);
            }
        };

        Refrescar();
    }

    private void Navegar(string url)
    {
        string? anterior = _historial.PaginaActual;
        try
        {
            if (_historial.Visitar(url))
            {
                Ui.Registrar(_lstBitacora, anterior is null
                    ? $"Primera visita: {_historial.PaginaActual}"
                    : $"Visitar {_historial.PaginaActual} → PUSH «{anterior}» en Atrás; se vacía Adelante");
            }
            else
            {
                Ui.Registrar(_lstBitacora, $"Recargar {_historial.PaginaActual} (el historial no cambia)");
            }
            _txtUrl.Clear();
        }
        catch (ArgumentException ex)
        {
            Ui.Aviso(this, ex.Message);
        }
        Refrescar();
    }

    private void Retroceder()
    {
        if (!_historial.PuedeRetroceder) return;
        string actual = _historial.PaginaActual!;
        string nueva = _historial.Retroceder();
        Ui.Registrar(_lstBitacora, $"Atrás: PUSH «{actual}» en Adelante, POP «{nueva}» de Atrás");
        Refrescar();
    }

    private void Avanzar()
    {
        if (!_historial.PuedeAvanzar) return;
        string actual = _historial.PaginaActual!;
        string nueva = _historial.Avanzar();
        Ui.Registrar(_lstBitacora, $"Adelante: PUSH «{actual}» en Atrás, POP «{nueva}» de Adelante");
        Refrescar();
    }

    private void Refrescar()
    {
        _lblPagina.Text = _historial.PaginaActual is null
            ? "Aún no has visitado ninguna página"
            : $"Página actual:  {_historial.PaginaActual}";

        _visorAtras.Mostrar(_historial.PilaAtras);
        _visorAdelante.Mostrar(_historial.PilaAdelante);
        _btnAtras.Enabled = _historial.PuedeRetroceder;
        _btnAdelante.Enabled = _historial.PuedeAvanzar;
    }
}
