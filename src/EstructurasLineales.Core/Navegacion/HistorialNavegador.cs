using EstructurasLineales.Core.Estructuras;

namespace EstructurasLineales.Core.Navegacion;

/// <summary>
/// Simula los botones "Atrás" y "Adelante" de un navegador web usando DOS pilas.
///  - Pila Atrás: páginas visitadas antes de la actual (tope = la anterior inmediata).
///  - Pila Adelante: páginas de las que se regresó (tope = la siguiente inmediata).
/// Visitar una página nueva vacía la pila Adelante, igual que en un navegador real.
/// </summary>
public class HistorialNavegador
{
    private readonly Pila<string> _atras = new();
    private readonly Pila<string> _adelante = new();

    /// <summary>Página que se está mostrando (null si aún no se visita ninguna).</summary>
    public string? PaginaActual { get; private set; }

    /// <summary>Contenido de la pila Atrás, desde el tope hasta el fondo.</summary>
    public IEnumerable<string> PilaAtras => _atras;

    /// <summary>Contenido de la pila Adelante, desde el tope hasta el fondo.</summary>
    public IEnumerable<string> PilaAdelante => _adelante;

    public bool PuedeRetroceder => !_atras.EstaVacia;
    public bool PuedeAvanzar => !_adelante.EstaVacia;

    /// <summary>Navega a una URL nueva.</summary>
    /// <returns>true si se navegó; false si la URL es la misma que la página actual.</returns>
    public bool Visitar(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("La URL no puede estar vacía.", nameof(url));

        string normalizada = Normalizar(url);
        if (string.Equals(normalizada, PaginaActual, StringComparison.OrdinalIgnoreCase))
            return false; // Recargar la misma página no altera el historial.

        if (PaginaActual is not null)
            _atras.Push(PaginaActual);

        PaginaActual = normalizada;
        _adelante.Limpiar(); // Se pierde el "futuro" al tomar un camino nuevo.
        return true;
    }

    /// <summary>Equivale a presionar el botón "Atrás".</summary>
    public string Retroceder()
    {
        if (!PuedeRetroceder)
            throw new InvalidOperationException("No hay páginas anteriores.");

        _adelante.Push(PaginaActual!);
        PaginaActual = _atras.Pop();
        return PaginaActual;
    }

    /// <summary>Equivale a presionar el botón "Adelante".</summary>
    public string Avanzar()
    {
        if (!PuedeAvanzar)
            throw new InvalidOperationException("No hay páginas siguientes.");

        _atras.Push(PaginaActual!);
        PaginaActual = _adelante.Pop();
        return PaginaActual;
    }

    /// <summary>Borra todo el historial.</summary>
    public void Reiniciar()
    {
        _atras.Limpiar();
        _adelante.Limpiar();
        PaginaActual = null;
    }

    /// <summary>Agrega "https://" si la URL no trae protocolo y elimina espacios.</summary>
    public static string Normalizar(string url)
    {
        string limpia = url.Trim();
        bool tieneProtocolo = limpia.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                           || limpia.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        return tieneProtocolo ? limpia : "https://" + limpia;
    }
}
