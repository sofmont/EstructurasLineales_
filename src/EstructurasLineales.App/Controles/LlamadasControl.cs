using System.ComponentModel;
using EstructurasLineales.Core.Llamadas;

namespace EstructurasLineales.App.Controles;

/// <summary>Escenario 2: simulación de la pila de llamadas (call stack) con factorial recursivo.</summary>
[DesignerCategory("Code")]
public class LlamadasControl : UserControl
{
    private const string CodigoSimulado =
        "long factorial(int n)\r\n" +
        "{\r\n" +
        "    if (n <= 1) return 1;          // caso base  → POP\r\n" +
        "    return n * factorial(n - 1);   // recursión  → PUSH\r\n" +
        "}";

    private readonly NumericUpDown _numN = new() { Minimum = 0, Maximum = 60, Value = 5, Width = 60, Margin = new Padding(2, 8, 8, 4) };
    private readonly NumericUpDown _numLimite = new() { Minimum = 3, Maximum = 60, Value = 20, Width = 60, Margin = new Padding(2, 8, 8, 4) };
    private readonly Button _btnGenerar = Ui.Boton("Generar simulación", Ui.Exito, 160);
    private readonly Button _btnPaso = Ui.Boton("Paso ▶", Ui.Primario, 100);
    private readonly Button _btnReproducir = Ui.Boton("Reproducir ▶▶", Ui.Primario, 130);
    private readonly Button _btnTodo = Ui.Boton("Ver todo", Ui.Neutro, 100);
    private readonly VisorPila _visor = new() { ColorTope = Ui.Primario };
    private readonly ListBox _lstPasos = Ui.Bitacora();
    private readonly Label _lblEstado = new()
    {
        Dock = DockStyle.Bottom,
        Height = 44,
        TextAlign = ContentAlignment.MiddleLeft,
        Font = new Font("Segoe UI", 11F, FontStyle.Bold),
        Padding = new Padding(8, 0, 0, 0),
        BackColor = Color.White
    };
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 700 };

    private IReadOnlyList<PasoLlamada> _pasos = Array.Empty<PasoLlamada>();
    private int _indice = -1;

    public LlamadasControl()
    {
        BackColor = Ui.Fondo;

        var barra = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true };   // crece si los botones no caben
        barra.Controls.Add(Ui.Etiqueta("factorial( n ) con n ="));
        barra.Controls.Add(_numN);
        barra.Controls.Add(Ui.Etiqueta("Límite de la pila (marcos):"));
        barra.Controls.Add(_numLimite);
        barra.Controls.Add(_btnGenerar);
        barra.Controls.Add(_btnPaso);
        barra.Controls.Add(_btnReproducir);
        barra.Controls.Add(_btnTodo);

        var codigo = new Label
        {
            Text = CodigoSimulado,
            Dock = DockStyle.Top,
            Height = 100,
            Font = new Font("Consolas", 10.5F),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.FromArgb(226, 232, 240),
            Padding = new Padding(10)
        };
        var panelDerecho = new Panel { Dock = DockStyle.Fill };
        panelDerecho.Controls.Add(Ui.Grupo("Secuencia de operaciones", _lstPasos));
        panelDerecho.Controls.Add(codigo);

        var cuerpo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 6, 0, 6) };
        cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        cuerpo.Controls.Add(Ui.Grupo("Pila de llamadas (call stack)", _visor), 0, 0);
        cuerpo.Controls.Add(panelDerecho, 1, 0);

        Controls.Add(cuerpo);
        Controls.Add(_lblEstado);
        Controls.Add(barra);

        _btnGenerar.Click += (_, _) => Generar();
        _btnPaso.Click += (_, _) => { _timer.Stop(); ActualizarBotonReproducir(); Avanzar(); };
        _btnReproducir.Click += (_, _) => AlternarReproduccion();
        _btnTodo.Click += (_, _) => VerTodo();
        _timer.Tick += (_, _) => Avanzar();
        _numN.ValueChanged += (_, _) => Generar();
        _numLimite.ValueChanged += (_, _) => Generar();

        Generar();
    }

    private bool Terminado => _indice >= _pasos.Count - 1;

    private void Generar()
    {
        _timer.Stop();
        var simulador = new SimuladorPilaLlamadas((int)_numLimite.Value);
        _pasos = simulador.SimularFactorial((int)_numN.Value);
        _indice = -1;
        _lstPasos.Items.Clear();
        _visor.ColorTope = Ui.Primario;
        _visor.Mostrar(Array.Empty<string>());
        _lblEstado.ForeColor = Ui.Oscuro;
        _lblEstado.Text = $"Simulación lista: {_pasos.Count} pasos. Usa «Paso» o «Reproducir».";
        ActualizarBotonReproducir();
    }

    private void Avanzar()
    {
        if (Terminado)
        {
            _timer.Stop();
            ActualizarBotonReproducir();
            return;
        }

        _indice++;
        PasoLlamada paso = _pasos[_indice];
        _lstPasos.Items.Add($"{_indice + 1,3}. {paso.Descripcion}");
        _lstPasos.TopIndex = Math.Max(0, _lstPasos.Items.Count - 1);

        _visor.ColorTope = paso.Tipo switch
        {
            TipoPaso.Llamada => Ui.Primario,
            TipoPaso.Retorno => Ui.Exito,
            _ => Ui.Peligro
        };
        _visor.Mostrar(paso.PilaActual);

        if (paso.Tipo == TipoPaso.Desbordamiento)
        {
            _lblEstado.ForeColor = Ui.Peligro;
            _lblEstado.Text = $"⚠ {paso.Descripcion}. La recursión es demasiado profunda.";
        }
        else if (Terminado)
        {
            _lblEstado.ForeColor = Ui.Exito;
            _lblEstado.Text = $"✔ Resultado: factorial({_numN.Value}) = {paso.Resultado}   (pila vacía, todas las llamadas regresaron)";
        }
        else
        {
            _lblEstado.ForeColor = Ui.Oscuro;
            _lblEstado.Text = $"Paso {_indice + 1} de {_pasos.Count} · Marcos en la pila: {paso.PilaActual.Count}";
        }

        if (Terminado)
        {
            _timer.Stop();
            ActualizarBotonReproducir();
        }
    }

    private void AlternarReproduccion()
    {
        if (_timer.Enabled)
        {
            _timer.Stop();
        }
        else
        {
            if (Terminado) Generar();
            _timer.Start();
        }
        ActualizarBotonReproducir();
    }

    private void VerTodo()
    {
        _timer.Stop();
        while (!Terminado) Avanzar();
        ActualizarBotonReproducir();
    }

    private void ActualizarBotonReproducir() =>
        _btnReproducir.Text = _timer.Enabled ? "Pausar ❚❚" : "Reproducir ▶▶";

    protected override void Dispose(bool disposing)
    {
        if (disposing) _timer.Dispose();
        base.Dispose(disposing);
    }
}
