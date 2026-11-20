using EstructurasLineales.Core.Estructuras;

namespace EstructurasLineales.Tests;

public class PilaTests
{
    [Fact] // CP-01
    public void Push_y_Pop_respetan_orden_LIFO()
    {
        var pila = new Pila<int>();
        pila.Push(1);
        pila.Push(2);
        pila.Push(3);

        Assert.Equal(3, pila.Pop());
        Assert.Equal(2, pila.Pop());
        Assert.Equal(1, pila.Pop());
        Assert.True(pila.EstaVacia);
    }

    [Fact] // CP-02
    public void Peek_devuelve_el_tope_sin_extraerlo()
    {
        var pila = new Pila<string>();
        pila.Push("a");
        pila.Push("b");

        Assert.Equal("b", pila.Peek());
        Assert.Equal(2, pila.Cantidad);
    }

    [Fact] // CP-03
    public void Pop_en_pila_vacia_lanza_excepcion()
    {
        var pila = new Pila<int>();
        Assert.Throws<InvalidOperationException>(() => pila.Pop());
        Assert.Throws<InvalidOperationException>(() => pila.Peek());
    }

    [Fact] // CP-04
    public void Recorrido_va_del_tope_al_fondo_y_Limpiar_vacia()
    {
        var pila = new Pila<int>();
        foreach (int n in new[] { 10, 20, 30 }) pila.Push(n);

        Assert.Equal(new[] { 30, 20, 10 }, pila.ToArray());

        pila.Limpiar();
        Assert.True(pila.EstaVacia);
        Assert.Equal(0, pila.Cantidad);
    }
}

public class ColaPrioridadTests
{
    [Fact] // CP-05
    public void Desencola_primero_la_prioridad_mas_urgente()
    {
        var cola = new ColaPrioridad<string>();
        cola.Encolar("normal", 3);
        cola.Encolar("urgente", 1);
        cola.Encolar("media", 2);

        Assert.Equal("urgente", cola.Desencolar());
        Assert.Equal("media", cola.Desencolar());
        Assert.Equal("normal", cola.Desencolar());
    }

    [Fact] // CP-06
    public void Con_igual_prioridad_respeta_orden_de_llegada_FIFO()
    {
        var cola = new ColaPrioridad<string>();
        cola.Encolar("A", 2);
        cola.Encolar("B", 2);
        cola.Encolar("X", 1);
        cola.Encolar("C", 2);

        Assert.Equal(new[] { "X", "A", "B", "C" }, cola.ToArray());
    }

    [Fact] // CP-07
    public void Desencolar_en_cola_vacia_lanza_excepcion()
    {
        var cola = new ColaPrioridad<int>();
        Assert.Throws<InvalidOperationException>(() => cola.Desencolar());
        Assert.Throws<InvalidOperationException>(() => cola.VerFrente());
    }

    [Fact] // CP-08
    public void Cantidad_se_actualiza_al_encolar_y_desencolar()
    {
        var cola = new ColaPrioridad<int>();
        cola.Encolar(1, 5);
        cola.Encolar(2, 5);
        Assert.Equal(2, cola.Cantidad);

        cola.Desencolar();
        Assert.Equal(1, cola.Cantidad);
        Assert.False(cola.EstaVacia);
    }
}
