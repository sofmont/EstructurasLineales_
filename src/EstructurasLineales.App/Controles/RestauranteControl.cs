using System.ComponentModel;
using EstructurasLineales.Core.Restaurantes;

namespace EstructurasLineales.App.Controles;

/// <summary>Escenario 3: lista de espera de un restaurante con cola de prioridad.</summary>
[DesignerCategory("Code")]
public class RestauranteControl : UserControl
{
    private sealed record OpcionTipo(TipoCliente Tipo)
    {
        public override string ToString() => $"{(int)Tipo} · {Tipo.Descripcion()}";
    }

    private readonly Restaurante _restaurante = new();
    private int _numeroMesa;

    private readonly TextBox _txtNombre = new() { Width = 170, Margin = new Padding(2, 8, 6, 4), PlaceholderText = "Nombre del cliente" };
    private readonly NumericUpDown _numPersonas = new() { Minimum = 1, Maximum = Restaurante.MaxPersonasPorMesa, Value = 2, Width = 55, Margin = new Padding(2, 8, 6, 4) };
    private readonly ComboBox _cmbTipo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 330, Margin = new Padding(2, 8, 6, 4) };
    private readonly Button _btnRegistrar = Ui.Boton("Registrar llegada", Ui.Exito, 150);
    private readonly Button _btnAsignar = Ui.Boton("Asignar mesa ▶", Ui.Primario, 150);
    private readonly Button _btnEjemplo = Ui.Boton("Cargar ejemplo", Ui.Neutro, 140);
    private readonly Button _btnReiniciar = Ui.Boton("Reiniciar", Ui.Neutro, 100);

    private readonly ListView _lvCola = Ui.Lista(
        ("Pos.", 50), ("Turno", 60), ("Nombre", 170), ("Personas", 75), ("Prioridad / tipo", 330), ("Llegada", 80));
    private readonly Label _lblSiguiente = Ui.TarjetaGrande();
    private readonly ListBox _lstAtendidos = Ui.Bitacora();
    private readonly Label _lblEstado = new() { Dock = DockStyle.Bottom, Height = 30, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(6, 0, 0, 0) };

    public RestauranteControl()
    {
        BackColor = Ui.Fondo;

        foreach (TipoCliente tipo in Enum.GetValues<TipoCliente>())
            _cmbTipo.Items.Add(new OpcionTipo(tipo));
        _cmbTipo.SelectedIndex = _cmbTipo.Items.Count - 1; // General por defecto

        var barra = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 0, 0, 4) };
        barra.Controls.Add(Ui.Etiqueta("Nombre:"));
        barra.Controls.Add(_txtNombre);
        barra.Controls.Add(Ui.Etiqueta("Personas:"));
        barra.Controls.Add(_numPersonas);
        barra.Controls.Add(Ui.Etiqueta("Tipo:"));
        barra.Controls.Add(_cmbTipo);
        barra.Controls.Add(_btnRegistrar);
        barra.Controls.Add(_btnAsignar);
        barra.Controls.Add(_btnEjemplo);
        barra.Controls.Add(_btnReiniciar);

        var derecha = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        derecha.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        derecha.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        derecha.Controls.Add(Ui.Grupo("Frente de la cola (siguiente en pasar)", _lblSiguiente), 0, 0);
        derecha.Controls.Add(Ui.Grupo("Mesas asignadas (Desencolar)", _lstAtendidos), 0, 1);

        var cuerpo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 4, 0, 4) };
        cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        cuerpo.Controls.Add(Ui.Grupo("Cola de espera — ordenada por prioridad (1 = más urgente) y luego por llegada", _lvCola), 0, 0);
        cuerpo.Controls.Add(derecha, 1, 0);

        Controls.Add(cuerpo);
        Controls.Add(_lblEstado);
        Controls.Add(barra);

        _btnRegistrar.Click += (_, _) => Registrar();
        _btnAsignar.Click += (_, _) => AsignarMesa();
        _btnEjemplo.Click += (_, _) => CargarEjemplo();
        _btnReiniciar.Click += (_, _) =>
        {
            _restaurante.Reiniciar();
            _numeroMesa = 0;
            _lstAtendidos.Items.Clear();
            Refrescar();
        };
        _txtNombre.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Registrar();
            }
        };

        Refrescar();
    }

    private void Registrar()
    {
        if (_cmbTipo.SelectedItem is not OpcionTipo opcion) return;
        try
        {
            _restaurante.RegistrarLlegada(_txtNombre.Text, (int)_numPersonas.Value, opcion.Tipo);
            _txtNombre.Clear();
            _txtNombre.Focus();
        }
        catch (ArgumentException ex)
        {
            Ui.Aviso(this, ex.Message);
        }
        Refrescar();
    }

    private void AsignarMesa()
    {
        if (_restaurante.CantidadEnEspera == 0)
        {
            Ui.Aviso(this, "No hay clientes en espera.");
            return;
        }

        Cliente cliente = _restaurante.AsignarMesa();
        _numeroMesa++;
        _lstAtendidos.Items.Insert(0,
            $"[{Ui.Hora(cliente.HoraAtencion ?? DateTime.Now)}] Mesa {_numeroMesa,2} → #{cliente.Turno} {cliente.Nombre} (P{cliente.Prioridad})");
        Refrescar();
    }

    private void CargarEjemplo()
    {
        // Orden de LLEGADA deliberadamente mezclado para demostrar la reordenación por prioridad.
        (string Nombre, int Personas, TipoCliente Tipo)[] llegadas =
        {
            ("Familia López", 4, TipoCliente.General),
            ("Ana Torres", 2, TipoCliente.Reservacion),
            ("Don Roberto", 1, TipoCliente.Preferente),
            ("Grupo oficina", 8, TipoCliente.General),
            ("Carlos Ruiz", 3, TipoCliente.Reservacion),
            ("Sra. Martínez", 2, TipoCliente.Preferente),
            ("Luis y Paola", 2, TipoCliente.General)
        };

        foreach (var (nombre, personas, tipo) in llegadas)
            _restaurante.RegistrarLlegada(nombre, personas, tipo);

        Refrescar();
    }

    private void Refrescar()
    {
        _lvCola.BeginUpdate();
        _lvCola.Items.Clear();
        int posicion = 1;
        foreach (Cliente c in _restaurante.EnEspera)
        {
            var fila = new ListViewItem(new[]
            {
                (posicion++).ToString(),
                $"#{c.Turno}",
                c.Nombre,
                c.Personas.ToString(),
                $"{c.Prioridad} · {c.Tipo.Descripcion()}",
                Ui.Hora(c.HoraLlegada)
            })
            {
                BackColor = ColorPorTipo(c.Tipo)
            };
            _lvCola.Items.Add(fila);
        }
        _lvCola.EndUpdate();

        Cliente? siguiente = _restaurante.Siguiente;
        _lblSiguiente.Text = siguiente is null
            ? "Nadie en espera"
            : $"#{siguiente.Turno}  {siguiente.Nombre}\r\n{siguiente.Personas} persona(s) · Prioridad {siguiente.Prioridad}";
        _lblSiguiente.BackColor = siguiente is null ? Color.White : ColorPorTipo(siguiente.Tipo);

        _btnAsignar.Enabled = siguiente is not null;
        _lblEstado.Text = $"En espera: {_restaurante.CantidadEnEspera}    ·    Atendidos: {_restaurante.Atendidos.Count}    ·    " +
                          "Colores: rojo = preferente, amarillo = reservación, blanco = general";
    }

    private static Color ColorPorTipo(TipoCliente tipo) => tipo switch
    {
        TipoCliente.Preferente => Color.FromArgb(254, 226, 226),
        TipoCliente.Reservacion => Color.FromArgb(254, 243, 199),
        _ => Color.White
    };
}
