using System.ComponentModel;
using EstructurasLineales.Core.Aeropuerto;

namespace EstructurasLineales.App.Controles;

/// <summary>Escenario 4: despacho de aviones en una pista con cola de prioridad.</summary>
[DesignerCategory("Code")]
public class TorreControlControl : UserControl
{
    private sealed record OpcionCategoria(CategoriaVuelo Categoria)
    {
        public override string ToString() => $"{(int)Categoria} · {Categoria.Descripcion()}";
    }

    private static readonly (string Prefijo, string Aerolinea)[] Aerolineas =
    {
        ("AM", "Aeroméxico"), ("Y4", "Volaris"), ("VB", "Viva Aerobus"), ("AA", "American Airlines"), ("UA", "United")
    };

    private readonly TorreControl _torre = new();
    private readonly Random _azar = new();

    private readonly TextBox _txtCodigo = new() { Width = 90, Margin = new Padding(2, 8, 6, 4), CharacterCasing = CharacterCasing.Upper, PlaceholderText = "AM123" };
    private readonly ComboBox _cmbAerolinea = new() { DropDownStyle = ComboBoxStyle.DropDown, Width = 150, Margin = new Padding(2, 8, 6, 4) };
    private readonly ComboBox _cmbCategoria = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 190, Margin = new Padding(2, 8, 6, 4) };
    private readonly Button _btnSolicitar = Ui.Boton("Solicitar pista", Ui.Exito, 130);
    private readonly Button _btnAutorizar = Ui.Boton("Autorizar siguiente ▶", Ui.Primario, 175);
    private readonly Button _btnEjemplo = Ui.Boton("Cargar ejemplo", Ui.Neutro, 130);
    private readonly Button _btnAuto = Ui.Boton("Simulación automática", Ui.Advertencia, 185);
    private readonly Button _btnReiniciar = Ui.Boton("Reiniciar", Ui.Neutro, 95);

    private readonly ListView _lvCola = Ui.Lista(
        ("Pos.", 50), ("Vuelo", 80), ("Aerolínea", 150), ("Prioridad / categoría", 200), ("Solicitud", 80));
    private readonly Label _lblSiguiente = Ui.TarjetaGrande();
    private readonly ListBox _lstBitacora = Ui.Bitacora();
    private readonly Label _lblEstado = new() { Dock = DockStyle.Bottom, Height = 30, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(6, 0, 0, 0) };
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1200 };

    public TorreControlControl()
    {
        BackColor = Ui.Fondo;

        foreach (var (_, aerolinea) in Aerolineas)
            _cmbAerolinea.Items.Add(aerolinea);
        _cmbAerolinea.SelectedIndex = 0;

        foreach (CategoriaVuelo categoria in Enum.GetValues<CategoriaVuelo>())
            _cmbCategoria.Items.Add(new OpcionCategoria(categoria));
        _cmbCategoria.SelectedIndex = 2; // Aterrizaje por defecto

        var barra = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 0, 0, 4) };
        barra.Controls.Add(Ui.Etiqueta("Vuelo:"));
        barra.Controls.Add(_txtCodigo);
        barra.Controls.Add(Ui.Etiqueta("Aerolínea:"));
        barra.Controls.Add(_cmbAerolinea);
        barra.Controls.Add(Ui.Etiqueta("Categoría:"));
        barra.Controls.Add(_cmbCategoria);
        barra.Controls.Add(_btnSolicitar);
        barra.Controls.Add(_btnAutorizar);
        barra.Controls.Add(_btnEjemplo);
        barra.Controls.Add(_btnAuto);
        barra.Controls.Add(_btnReiniciar);

        var derecha = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        derecha.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        derecha.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        derecha.Controls.Add(Ui.Grupo("Frente de la cola (siguiente en usar la pista)", _lblSiguiente), 0, 0);
        derecha.Controls.Add(Ui.Grupo("Bitácora de la torre", _lstBitacora), 0, 1);

        var cuerpo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 4, 0, 4) };
        cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        cuerpo.Controls.Add(Ui.Grupo("Vuelos esperando pista — orden de atención", _lvCola), 0, 0);
        cuerpo.Controls.Add(derecha, 1, 0);

        Controls.Add(cuerpo);
        Controls.Add(_lblEstado);
        Controls.Add(barra);

        _btnSolicitar.Click += (_, _) => Solicitar();
        _btnAutorizar.Click += (_, _) => Autorizar();
        _btnEjemplo.Click += (_, _) => CargarEjemplo();
        _btnAuto.Click += (_, _) => AlternarSimulacion();
        _btnReiniciar.Click += (_, _) =>
        {
            _timer.Stop();
            _btnAuto.Text = "Simulación automática";
            _torre.Reiniciar();
            _lstBitacora.Items.Clear();
            Refrescar();
        };
        _txtCodigo.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Solicitar();
            }
        };
        _timer.Tick += (_, _) => PasoSimulacion();

        Refrescar();
    }

    private void Solicitar()
    {
        if (_cmbCategoria.SelectedItem is not OpcionCategoria opcion) return;
        try
        {
            Vuelo vuelo = _torre.SolicitarPista(_txtCodigo.Text, _cmbAerolinea.Text, opcion.Categoria);
            Ui.Registrar(_lstBitacora, $"ENCOLAR  {vuelo}");
            _txtCodigo.Clear();
            _txtCodigo.Focus();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Ui.Aviso(this, ex.Message);
        }
        Refrescar();
    }

    private void Autorizar()
    {
        if (_torre.CantidadPendientes == 0)
        {
            Ui.Aviso(this, "No hay vuelos esperando pista.");
            return;
        }
        Vuelo vuelo = _torre.AutorizarSiguiente();
        Ui.Registrar(_lstBitacora, $"PISTA ✈  {vuelo}");
        Refrescar();
    }

    private void CargarEjemplo()
    {
        (string Codigo, string Aerolinea, CategoriaVuelo Categoria)[] solicitudes =
        {
            ("Y4521", "Volaris", CategoriaVuelo.Despegue),
            ("AM123", "Aeroméxico", CategoriaVuelo.Aterrizaje),
            ("VB1002", "Viva Aerobus", CategoriaVuelo.Despegue),
            ("AA900", "American Airlines", CategoriaVuelo.CombustibleBajo),
            ("AM450", "Aeroméxico", CategoriaVuelo.Aterrizaje),
            ("UA77", "United", CategoriaVuelo.Emergencia),
            ("Y4330", "Volaris", CategoriaVuelo.Aterrizaje)
        };

        foreach (var (codigo, aerolinea, categoria) in solicitudes)
        {
            try
            {
                Vuelo vuelo = _torre.SolicitarPista(codigo, aerolinea, categoria);
                Ui.Registrar(_lstBitacora, $"ENCOLAR  {vuelo}");
            }
            catch (InvalidOperationException)
            {
                // El vuelo ya estaba en la cola (se presionó "Cargar ejemplo" dos veces).
            }
        }
        Refrescar();
    }

    private void AlternarSimulacion()
    {
        if (_timer.Enabled)
        {
            _timer.Stop();
            _btnAuto.Text = "Simulación automática";
        }
        else
        {
            _timer.Start();
            _btnAuto.Text = "Detener simulación";
        }
    }

    /// <summary>Cada tic llega un vuelo aleatorio y, con cierta probabilidad, se libera la pista.</summary>
    private void PasoSimulacion()
    {
        var (prefijo, aerolinea) = Aerolineas[_azar.Next(Aerolineas.Length)];
        string codigo = $"{prefijo}{_azar.Next(100, 10000)}";

        int dado = _azar.Next(100);
        CategoriaVuelo categoria = dado switch
        {
            < 5 => CategoriaVuelo.Emergencia,
            < 15 => CategoriaVuelo.CombustibleBajo,
            < 60 => CategoriaVuelo.Aterrizaje,
            _ => CategoriaVuelo.Despegue
        };

        try
        {
            Vuelo vuelo = _torre.SolicitarPista(codigo, aerolinea, categoria);
            Ui.Registrar(_lstBitacora, $"ENCOLAR  {vuelo}");
        }
        catch (InvalidOperationException)
        {
            // Código repetido por azar: se ignora este tic.
        }

        if (_torre.CantidadPendientes > 0 && _azar.NextDouble() < 0.65)
        {
            Vuelo autorizado = _torre.AutorizarSiguiente();
            Ui.Registrar(_lstBitacora, $"PISTA ✈  {autorizado}");
        }

        Refrescar();
    }

    private void Refrescar()
    {
        _lvCola.BeginUpdate();
        _lvCola.Items.Clear();
        int posicion = 1;
        foreach (Vuelo v in _torre.Pendientes)
        {
            var fila = new ListViewItem(new[]
            {
                (posicion++).ToString(),
                v.Codigo,
                v.Aerolinea,
                $"{v.Prioridad} · {v.Categoria.Descripcion()}",
                Ui.Hora(v.HoraSolicitud)
            })
            {
                BackColor = ColorPorCategoria(v.Categoria)
            };
            _lvCola.Items.Add(fila);
        }
        _lvCola.EndUpdate();

        Vuelo? siguiente = _torre.Siguiente;
        _lblSiguiente.Text = siguiente is null
            ? "Pista libre — sin vuelos en espera"
            : $"✈  {siguiente.Codigo}  ({siguiente.Aerolinea})\r\n{siguiente.Categoria.Descripcion()} · Prioridad {siguiente.Prioridad}";
        _lblSiguiente.BackColor = siguiente is null ? Color.White : ColorPorCategoria(siguiente.Categoria);

        _btnAutorizar.Enabled = siguiente is not null;
        _lblEstado.Text = $"En espera: {_torre.CantidadPendientes}    ·    Operaciones autorizadas: {_torre.Autorizados.Count}    ·    " +
                          "Orden: Emergencia → Combustible bajo → Aterrizaje → Despegue";
    }

    private static Color ColorPorCategoria(CategoriaVuelo categoria) => categoria switch
    {
        CategoriaVuelo.Emergencia => Color.FromArgb(254, 202, 202),
        CategoriaVuelo.CombustibleBajo => Color.FromArgb(254, 215, 170),
        CategoriaVuelo.Aterrizaje => Color.FromArgb(219, 234, 254),
        _ => Color.FromArgb(220, 252, 231)
    };

    protected override void Dispose(bool disposing)
    {
        if (disposing) _timer.Dispose();
        base.Dispose(disposing);
    }
}
