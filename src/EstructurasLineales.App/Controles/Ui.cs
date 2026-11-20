namespace EstructurasLineales.App.Controles;

/// <summary>Colores y fábricas de controles para que toda la interfaz tenga el mismo estilo.</summary>
internal static class Ui
{
    public static readonly Color Primario = Color.FromArgb(37, 99, 235);
    public static readonly Color Exito = Color.FromArgb(22, 163, 74);
    public static readonly Color Peligro = Color.FromArgb(220, 38, 38);
    public static readonly Color Advertencia = Color.FromArgb(217, 119, 6);
    public static readonly Color Neutro = Color.FromArgb(100, 116, 139);
    public static readonly Color Oscuro = Color.FromArgb(15, 23, 42);
    public static readonly Color Fondo = Color.FromArgb(241, 245, 249);

    /// <summary>
    /// Botón de color. <paramref name="ancho"/> es el ancho mínimo: si el texto no cabe
    /// (por ejemplo, con la pantalla al 125 % o 150 %), el botón crece en lugar de cortarlo.
    /// </summary>
    public static Button Boton(string texto, Color color, int ancho = 150)
    {
        var boton = new Button
        {
            Text = texto,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(ancho, 34),
            Padding = new Padding(6, 0, 6, 0),
            Margin = new Padding(4),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            UseVisualStyleBackColor = false
        };
        boton.FlatAppearance.BorderSize = 0;
        return boton;
    }

    public static Label Etiqueta(string texto) => new()
    {
        Text = texto,
        AutoSize = true,
        Margin = new Padding(8, 11, 2, 4)
    };

    public static GroupBox Grupo(string titulo, Control contenido)
    {
        var grupo = new GroupBox
        {
            Text = titulo,
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            BackColor = Color.White
        };
        contenido.Dock = DockStyle.Fill;
        contenido.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        grupo.Controls.Add(contenido);
        return grupo;
    }

    public static ListView Lista(params (string Titulo, int Ancho)[] columnas)
    {
        var lista = new ListView
        {
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            HideSelection = false,
            MultiSelect = false,
            BorderStyle = BorderStyle.None
        };
        foreach (var (titulo, ancho) in columnas)
            lista.Columns.Add(titulo, ancho);
        return lista;
    }

    public static ListBox Bitacora() => new()
    {
        IntegralHeight = false,
        BorderStyle = BorderStyle.None,
        Font = new Font("Consolas", 9.5F)
    };

    public static Label TarjetaGrande() => new()
    {
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Segoe UI", 13F, FontStyle.Bold),
        BackColor = Color.White,
        AutoEllipsis = true
    };

    public static void Aviso(IWin32Window dueno, string mensaje) =>
        MessageBox.Show(dueno, mensaje, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public static string Hora(DateTime fecha) => fecha.ToString("HH:mm:ss");

    /// <summary>Inserta una línea al inicio de la bitácora con la hora actual.</summary>
    public static void Registrar(ListBox bitacora, string texto) =>
        bitacora.Items.Insert(0, $"[{Hora(DateTime.Now)}] {texto}");
}
