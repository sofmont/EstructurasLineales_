using System.Collections;

namespace EstructurasLineales.Core.Estructuras;

/// <summary>
/// Pila (Stack) genérica implementada con nodos enlazados.
/// Política LIFO: el último elemento que entra es el primero que sale.
/// Todas las operaciones principales son O(1).
/// </summary>
public class Pila<T> : IEnumerable<T>
{
    private sealed class Nodo
    {
        public T Valor { get; }
        public Nodo? Siguiente { get; }

        public Nodo(T valor, Nodo? siguiente)
        {
            Valor = valor;
            Siguiente = siguiente;
        }
    }

    private Nodo? _tope;

    /// <summary>Número de elementos almacenados.</summary>
    public int Cantidad { get; private set; }

    /// <summary>Indica si la pila no tiene elementos.</summary>
    public bool EstaVacia => _tope is null;

    /// <summary>Inserta un elemento en el tope. O(1)</summary>
    public void Push(T valor)
    {
        _tope = new Nodo(valor, _tope);
        Cantidad++;
    }

    /// <summary>Extrae y devuelve el elemento del tope. O(1)</summary>
    /// <exception cref="InvalidOperationException">Si la pila está vacía (subdesbordamiento).</exception>
    public T Pop()
    {
        if (_tope is null)
            throw new InvalidOperationException("La pila está vacía.");

        T valor = _tope.Valor;
        _tope = _tope.Siguiente;
        Cantidad--;
        return valor;
    }

    /// <summary>Devuelve el elemento del tope sin extraerlo. O(1)</summary>
    /// <exception cref="InvalidOperationException">Si la pila está vacía.</exception>
    public T Peek()
    {
        if (_tope is null)
            throw new InvalidOperationException("La pila está vacía.");

        return _tope.Valor;
    }

    /// <summary>Elimina todos los elementos. O(1)</summary>
    public void Limpiar()
    {
        _tope = null;
        Cantidad = 0;
    }

    /// <summary>Recorre la pila desde el tope hasta el fondo.</summary>
    public IEnumerator<T> GetEnumerator()
    {
        for (Nodo? actual = _tope; actual is not null; actual = actual.Siguiente)
            yield return actual.Valor;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
