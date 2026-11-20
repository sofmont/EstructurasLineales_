using System.ComponentModel;
using EstructurasLineales.App.Controles;

namespace EstructurasLineales.App;

/// <summary>Ventana principal: una pestaña por cada escenario de la práctica.</summary>
[DesignerCategory("Code")]
public class FormPrincipal : Form
{
    // Datos que aparecen en el encabezado de la ventana.
    private const string Alumno = "Barbara Sofia Montalvo Ontiveros";
    private const string Materia = "Estructuras de Datos";

    public FormPrincipal()
    {
        Text = "Práctica 3.3 – Estructuras lineales: Pilas y Colas de prioridad";
        Size = new Size(1180, 760);
        MinimumSize = new Size(960, 640);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.5F);
        BackColor = Ui.Fondo;

        // Encabezado
        var encabezado = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Ui.Oscuro };
        var lblTitulo = new Label
        {
            Text = "Resolución de casos reales con estructuras lineales",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(14, 6)
        };
        var lblSub = new Label
        {
            Text = $"{Materia}  ·  Tarea 3.3  ·  {Alumno}",
            ForeColor = Color.FromArgb(203, 213, 225),
            AutoSize = true,
            Location = new Point(16, 34)
        };
        encabezado.Controls.Add(lblTitulo);
        encabezado.Controls.Add(lblSub);

        // Pestañas
        var tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(14, 6) };
        tabs.TabPages.Add(CrearPestana("Pila · Historial de navegación", new NavegadorControl()));
        tabs.TabPages.Add(CrearPestana("Pila · Llamadas a funciones", new LlamadasControl()));
        tabs.TabPages.Add(CrearPestana("Cola de prioridad · Restaurante", new RestauranteControl()));
        tabs.TabPages.Add(CrearPestana("Cola de prioridad · Torre de control", new TorreControlControl()));

        // Barra de estado
        var estado = new StatusStrip();
        estado.Items.Add(new ToolStripStatusLabel("Estructuras implementadas desde cero con nodos enlazados: Pila<T> y ColaPrioridad<T>"));

        // Orden de acoplamiento: el último agregado se acopla primero (queda en el borde).
        Controls.Add(tabs);
        Controls.Add(estado);
        Controls.Add(encabezado);
    }

    private static TabPage CrearPestana(string titulo, Control contenido)
    {
        var pagina = new TabPage(titulo) { BackColor = Ui.Fondo, Padding = new Padding(6) };
        contenido.Dock = DockStyle.Fill;
        pagina.Controls.Add(contenido);
        return pagina;
    }
}
