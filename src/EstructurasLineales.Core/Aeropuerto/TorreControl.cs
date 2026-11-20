using System.Text.RegularExpressions;
using EstructurasLineales.Core.Estructuras;

namespace EstructurasLineales.Core.Aeropuerto;

/// <summary>Categoría de la solicitud de pista. El valor numérico ES la prioridad.</summary>
public enum CategoriaVuelo
{
    Emergencia = 1,      // Falla mecánica, emergencia médica a bordo, etc.
    CombustibleBajo = 2, // Debe aterrizar cuanto antes
    Aterrizaje = 3,      // Aterrizaje normal (un avión en el aire gasta combustible)
    Despegue = 4         // Despegue normal (puede esperar en tierra)
}

public static class CategoriaVueloExtensiones
{
    public static string Descripcion(this CategoriaVuelo categoria) => categoria switch
    {
        CategoriaVuelo.Emergencia => "EMERGENCIA",
        CategoriaVuelo.CombustibleBajo => "Combustible bajo",
        CategoriaVuelo.Aterrizaje => "Aterrizaje",
        CategoriaVuelo.Despegue => "Despegue",
        _ => categoria.ToString()
    };
}

public class Vuelo
{
    public int Secuencia { get; }
    public string Codigo { get; }
    public string Aerolinea { get; }
    public CategoriaVuelo Categoria { get; }
    public DateTime HoraSolicitud { get; }
    public DateTime? HoraAutorizacion { get; internal set; }

    public int Prioridad => (int)Categoria;

    public Vuelo(int secuencia, string codigo, string aerolinea, CategoriaVuelo categoria, DateTime horaSolicitud)
    {
        Secuencia = secuencia;
        Codigo = codigo;
        Aerolinea = aerolinea;
        Categoria = categoria;
        HoraSolicitud = horaSolicitud;
    }

    public override string ToString() => $"{Codigo} ({Aerolinea}) - {Categoria.Descripcion()}";
}

/// <summary>
/// Torre de control que asigna UNA pista usando una COLA DE PRIORIDAD.
/// Orden: Emergencia → Combustible bajo → Aterrizajes → Despegues; empates por orden de solicitud.
/// </summary>
public class TorreControl
{
    private static readonly Regex FormatoCodigo = new("^[A-Z0-9]{2,3}[0-9]{1,4}$", RegexOptions.Compiled);

    private readonly ColaPrioridad<Vuelo> _cola = new();
    private readonly List<Vuelo> _autorizados = new();
    private readonly Func<DateTime> _reloj;
    private int _secuencia = 1;

    public TorreControl(Func<DateTime>? reloj = null)
    {
        _reloj = reloj ?? (() => DateTime.Now);
    }

    public IEnumerable<Vuelo> Pendientes => _cola;
    public IReadOnlyList<Vuelo> Autorizados => _autorizados;
    public int CantidadPendientes => _cola.Cantidad;
    public Vuelo? Siguiente => _cola.EstaVacia ? null : _cola.VerFrente();

    /// <summary>Un vuelo solicita la pista; se forma según su categoría.</summary>
    public Vuelo SolicitarPista(string codigo, string aerolinea, CategoriaVuelo categoria)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código de vuelo es obligatorio.", nameof(codigo));
        if (string.IsNullOrWhiteSpace(aerolinea))
            throw new ArgumentException("La aerolínea es obligatoria.", nameof(aerolinea));
        if (!Enum.IsDefined(categoria))
            throw new ArgumentOutOfRangeException(nameof(categoria), "Categoría no válida.");

        string cod = codigo.Trim().ToUpperInvariant();
        if (!FormatoCodigo.IsMatch(cod))
            throw new ArgumentException("Código inválido. Ejemplos válidos: AM123, Y4521, VB1002.", nameof(codigo));
        if (_cola.Any(v => v.Codigo == cod))
            throw new InvalidOperationException($"El vuelo {cod} ya está esperando pista.");

        var vuelo = new Vuelo(_secuencia++, cod, aerolinea.Trim(), categoria, _reloj());
        _cola.Encolar(vuelo, vuelo.Prioridad);
        return vuelo;
    }

    /// <summary>Autoriza el uso de la pista al vuelo más prioritario.</summary>
    public Vuelo AutorizarSiguiente()
    {
        if (_cola.EstaVacia)
            throw new InvalidOperationException("No hay vuelos esperando pista.");

        Vuelo vuelo = _cola.Desencolar();
        vuelo.HoraAutorizacion = _reloj();
        _autorizados.Add(vuelo);
        return vuelo;
    }

    public void Reiniciar()
    {
        _cola.Limpiar();
        _autorizados.Clear();
        _secuencia = 1;
    }
}
