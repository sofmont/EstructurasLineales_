namespace EstructurasLineales.App;

internal static class Program
{
    /// <summary>Punto de entrada de la aplicación.</summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FormPrincipal());
    }
}
