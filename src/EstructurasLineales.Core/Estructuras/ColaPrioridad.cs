using System.Collections;

namespace EstructurasLineales.Core.Estructuras;

/// <summary>
/// Cola de prioridad genérica implementada como lista enlazada ordenada.
/// - Un número de prioridad MENOR significa MAYOR urgencia (1 se atiende antes que 3).
/// - Ante la misma prioridad se respeta el orden de llegada (FIFO), es decir, la cola es estable.
/// Encolar: O(n) (busca la posición).  Desencolar / VerFrente: O(1).
/// </summary>
public class ColaPrioridad<T> : IEnumerable<T>
{
    private sealed class Nodo
    {
        public T Valor { get; }
        public int Prioridad { get; }
        public Nodo? Siguiente { get; set; }

        public Nodo(T valor, int prioridad)
        {
            Valor = valor;
            Prioridad = prioridad;
        }
    }

    private Nodo? _frente;

    /// <summary>Número de elementos en la cola.</summary>
    public int Cantidad { get; private set; }

    /// <summary>Indica si la cola no tiene elementos.</summary>
    public bool EstaVacia => _frente is null;

    /// <summary>
    /// Inserta un elemento en la posición que le corresponde según su prioridad.
    /// Se coloca DETRÁS de todos los que tienen prioridad igual o más urgente (estabilidad FIFO).
    /// </summary>
    public void Encolar(T valor, int prioridad)
    {
        var nuevo = new Nodo(valor, prioridad);

        if (_frente is null || prioridad < _frente.Prioridad)
        {
            // Va al frente: la cola está vacía o el nuevo es más urgente que el actual frente.
            nuevo.Siguiente = _frente;
            _frente = nuevo;
        }
        else
        {
            Nodo actual = _frente;
            while (actual.Siguiente is not null && actual.Siguiente.Prioridad <= prioridad)
                actual = actual.Siguiente;

            nuevo.Siguiente = actual.Siguiente;
            actual.Siguiente = nuevo;
        }

        Cantidad++;
    }

    /// <summary>Extrae y devuelve el elemento más prioritario. O(1)</summary>
    /// <exception cref="InvalidOperationException">Si la cola está vacía.</exception>
    public T Desencolar()
    {
        if (_frente is null)
            throw new InvalidOperationException("La cola está vacía.");

        T valor = _frente.Valor;
        _frente = _frente.Siguiente;
        Cantidad--;
        return valor;
    }

    /// <summary>Devuelve el elemento más prioritario sin extraerlo. O(1)</summary>
    /// <exception cref="InvalidOperationException">Si la cola está vacía.</exception>
    public T VerFrente()
    {
        if (_frente is null)
            throw new InvalidOperationException("La cola está vacía.");

        return _frente.Valor;
    }

    /// <summary>Elimina todos los elementos.</summary>
    public void Limpiar()
    {
        _frente = null;
        Cantidad = 0;
    }

    /// <summary>Recorre la cola en el orden en que serán atendidos los elementos.</summary>
    public IEnumerator<T> GetEnumerator()
    {
        for (Nodo? actual = _frente; actual is not null; actual = actual.Siguiente)
            yield return actual.Valor;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
