using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace EstructurasLineales.App.Controles;

/// <summary>
/// Dibuja una pila como bloques apilados verticalmente: el tope arriba y resaltado.
/// Se usa para mostrar gráficamente las pilas Atrás/Adelante y la pila de llamadas.
/// </summary>
[DesignerCategory("Code")]
public class VisorPila : Control
{
    private List<string> _elementos = new(); // índice 0 = tope

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color ColorTope { get; set; } = Ui.Primario;

    public VisorPila()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5F);
    }

    /// <summary>Muestra los elementos recibidos (ordenados del tope al fondo).</summary>
    public void Mostrar(IEnumerable<string> desdeElTope)
    {
        _elementos = desdeElTope.ToList();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        const int altoBloque = 34;
        const int margen = 10;
        const int espacioEtiqueta = 70;
        int anchoBloque = Math.Max(60, Width - 2 * margen - espacioEtiqueta);

        if (_elementos.Count == 0)
        {
            TextRenderer.DrawText(g, "(pila vacía)", Font, ClientRectangle, Color.Gray,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            return;
        }

        // ¿Cuántos bloques caben? Si no caben todos, se muestran los más cercanos al tope.
        int caben = Math.Max(1, (Height - 2 * margen) / altoBloque);
        int ocultos = Math.Max(0, _elementos.Count - caben);
        if (ocultos > 0) caben = Math.Max(1, caben - 1); // reservar una fila para el aviso
        ocultos = Math.Max(0, _elementos.Count - caben);

        int y = Height - margen - altoBloque;

        if (ocultos > 0)
        {
            var avisoRect = new Rectangle(margen, y, anchoBloque, altoBloque - 6);
            TextRenderer.DrawText(g, $"… {ocultos} elemento(s) más abajo", Font, avisoRect, Color.Gray,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            y -= altoBloque;
        }

        int visibles = _elementos.Count - ocultos;
        for (int i = visibles - 1; i >= 0; i--) // dibuja de abajo hacia arriba
        {
            bool esTope = i == 0;
            var rect = new Rectangle(margen, y, anchoBloque, altoBloque - 6);

            using (var relleno = new SolidBrush(esTope ? ColorTope : Color.FromArgb(226, 232, 240)))
                g.FillRectangle(relleno, rect);
            using (var borde = new Pen(esTope ? ColorTope : Color.FromArgb(148, 163, 184)))
                g.DrawRectangle(borde, rect);

            TextRenderer.DrawText(g, _elementos[i], Font, rect, esTope ? Color.White : Color.Black,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            if (esTope)
            {
                using var negrita = new Font(Font, FontStyle.Bold);
                TextRenderer.DrawText(g, "← TOPE", negrita, new Point(rect.Right + 6, rect.Y + 6), ColorTope);
            }

            y -= altoBloque;
        }
    }
}
