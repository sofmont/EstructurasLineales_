using EstructurasLineales.Core.Estructuras;

namespace EstructurasLineales.Core.Restaurantes;

/// <summary>Tipo de cliente. El valor numérico ES la prioridad (1 = se atiende primero).</summary>
public enum TipoCliente
{
    Preferente = 1,   // Adulto mayor, embarazada o persona con discapacidad
    Reservacion = 2,  // Llegó con reservación
    General = 3       // Llegó sin reservación
}

public static class TipoClienteExtensiones
{
    public static string Descripcion(this TipoCliente tipo) => tipo switch
    {
        TipoCliente.Preferente => "Preferente (adulto mayor / embarazada / discapacidad)",
        TipoCliente.Reservacion => "Con reservación",
        TipoCliente.General => "Sin reservación",
        _ => tipo.ToString()
    };
}

public class Cliente
{
    public int Turno { get; }
    public string Nombre { get; }
    public int Personas { get; }
    public TipoCliente Tipo { get; }
    public DateTime HoraLlegada { get; }
    public DateTime? HoraAtencion { get; internal set; }

    public int Prioridad => (int)Tipo;

    public Cliente(int turno, string nombre, int personas, TipoCliente tipo, DateTime horaLlegada)
    {
        Turno = turno;
        Nombre = nombre;
        Personas = personas;
        Tipo = tipo;
        HoraLlegada = horaLlegada;
    }

    public override string ToString() => $"#{Turno} {Nombre} ({Personas} pers.) - {Tipo.Descripcion()}";
}

/// <summary>
/// Lista de espera de un restaurante administrada con una COLA DE PRIORIDAD.
/// Primero pasan los clientes preferentes, luego los que tienen reservación y al final los generales;
/// dentro de cada grupo se respeta el orden de llegada.
/// </summary>
public class Restaurante
{
    public const int MaxPersonasPorMesa = 12;

    private readonly ColaPrioridad<Cliente> _espera = new();
    private readonly List<Cliente> _atendidos = new();
    private readonly Func<DateTime> _reloj;
    private int _siguienteTurno = 1;

    /// <param name="reloj">Fuente de la hora (permite pruebas deterministas). Por defecto DateTime.Now.</param>
    public Restaurante(Func<DateTime>? reloj = null)
    {
        _reloj = reloj ?? (() => DateTime.Now);
    }

    public IEnumerable<Cliente> EnEspera => _espera;
    public IReadOnlyList<Cliente> Atendidos => _atendidos;
    public int CantidadEnEspera => _espera.Cantidad;
    public Cliente? Siguiente => _espera.EstaVacia ? null : _espera.VerFrente();

    /// <summary>Registra a un cliente que llega y lo forma en la lista de espera.</summary>
    public Cliente RegistrarLlegada(string nombre, int personas, TipoCliente tipo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        if (personas < 1 || personas > MaxPersonasPorMesa)
            throw new ArgumentOutOfRangeException(nameof(personas), $"El número de personas debe estar entre 1 y {MaxPersonasPorMesa}.");
        if (!Enum.IsDefined(tipo))
            throw new ArgumentOutOfRangeException(nameof(tipo), "Tipo de cliente no válido.");

        var cliente = new Cliente(_siguienteTurno++, nombre.Trim(), personas, tipo, _reloj());
        _espera.Encolar(cliente, cliente.Prioridad);
        return cliente;
    }

    /// <summary>Asigna mesa al cliente más prioritario (lo saca de la cola).</summary>
    public Cliente AsignarMesa()
    {
        if (_espera.EstaVacia)
            throw new InvalidOperationException("No hay clientes en espera.");

        Cliente cliente = _espera.Desencolar();
        cliente.HoraAtencion = _reloj();
        _atendidos.Add(cliente);
        return cliente;
    }

    public void Reiniciar()
    {
        _espera.Limpiar();
        _atendidos.Clear();
        _siguienteTurno = 1;
    }
}
