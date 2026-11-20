using EstructurasLineales.Core.Aeropuerto;
using EstructurasLineales.Core.Llamadas;
using EstructurasLineales.Core.Navegacion;
using EstructurasLineales.Core.Restaurantes;

namespace EstructurasLineales.Tests;

public class HistorialNavegadorTests
{
    [Fact] // CP-09
    public void Atras_regresa_a_la_pagina_anterior()
    {
        var h = new HistorialNavegador();
        h.Visitar("google.com");
        h.Visitar("github.com");
        h.Visitar("youtube.com");

        Assert.Equal("https://github.com", h.Retroceder());
        Assert.Equal("https://google.com", h.Retroceder());
        Assert.False(h.PuedeRetroceder);
    }

    [Fact] // CP-10
    public void Adelante_recupera_la_pagina_de_la_que_se_regreso()
    {
        var h = new HistorialNavegador();
        h.Visitar("a.com");
        h.Visitar("b.com");
        h.Retroceder();

        Assert.True(h.PuedeAvanzar);
        Assert.Equal("https://b.com", h.Avanzar());
        Assert.False(h.PuedeAvanzar);
    }

    [Fact] // CP-11
    public void Visitar_pagina_nueva_vacia_la_pila_adelante()
    {
        var h = new HistorialNavegador();
        h.Visitar("a.com");
        h.Visitar("b.com");
        h.Retroceder();          // actual = a.com, adelante = [b.com]
        h.Visitar("c.com");      // camino nuevo

        Assert.False(h.PuedeAvanzar);
        Assert.Equal(new[] { "https://a.com" }, h.PilaAtras.ToArray());
    }

    [Fact] // CP-12
    public void Retroceder_sin_historial_lanza_excepcion()
    {
        var h = new HistorialNavegador();
        h.Visitar("unica.com");
        Assert.Throws<InvalidOperationException>(() => h.Retroceder());
        Assert.Throws<InvalidOperationException>(() => h.Avanzar());
    }

    [Fact] // CP-13
    public void Url_vacia_es_rechazada_y_misma_url_no_altera_historial()
    {
        var h = new HistorialNavegador();
        Assert.Throws<ArgumentException>(() => h.Visitar("   "));

        h.Visitar("google.com");
        Assert.False(h.Visitar("https://google.com"));
        Assert.False(h.PuedeRetroceder);
    }
}

public class SimuladorPilaLlamadasTests
{
    [Fact] // CP-14
    public void Factorial_de_5_termina_en_120_con_pila_vacia()
    {
        var pasos = new SimuladorPilaLlamadas().SimularFactorial(5);
        var ultimo = pasos[^1];

        Assert.Equal(TipoPaso.Retorno, ultimo.Tipo);
        Assert.Equal(120, (int)ultimo.Resultado!.Value);
        Assert.Empty(ultimo.PilaActual);
        Assert.Equal(10, pasos.Count); // 5 PUSH + 5 POP
    }

    [Fact] // CP-15
    public void La_profundidad_maxima_es_igual_a_n()
    {
        var pasos = new SimuladorPilaLlamadas().SimularFactorial(4);
        int maximo = pasos.Max(p => p.PilaActual.Count);

        Assert.Equal(4, maximo);
        Assert.Equal("factorial(1)", pasos[3].PilaActual[0]); // tope en el caso base
    }

    [Fact] // CP-16
    public void Recursion_muy_profunda_produce_stack_overflow()
    {
        var pasos = new SimuladorPilaLlamadas(limiteProfundidad: 10).SimularFactorial(30);

        Assert.Equal(TipoPaso.Desbordamiento, pasos[^1].Tipo);
        Assert.Equal(10, pasos[^1].PilaActual.Count);
    }
}

public class RestauranteTests
{
    [Fact] // CP-17
    public void Clientes_preferentes_pasan_antes_aunque_lleguen_despues()
    {
        var r = new Restaurante();
        r.RegistrarLlegada("General 1", 2, TipoCliente.General);
        r.RegistrarLlegada("Reserva 1", 4, TipoCliente.Reservacion);
        r.RegistrarLlegada("Preferente 1", 1, TipoCliente.Preferente);
        r.RegistrarLlegada("General 2", 3, TipoCliente.General);

        Assert.Equal("Preferente 1", r.AsignarMesa().Nombre);
        Assert.Equal("Reserva 1", r.AsignarMesa().Nombre);
        Assert.Equal("General 1", r.AsignarMesa().Nombre);
        Assert.Equal("General 2", r.AsignarMesa().Nombre);
        Assert.Equal(4, r.Atendidos.Count);
    }

    [Theory] // CP-18
    [InlineData("", 2)]
    [InlineData("Ana", 0)]
    [InlineData("Ana", 13)]
    public void Datos_invalidos_son_rechazados(string nombre, int personas)
    {
        var r = new Restaurante();
        Assert.ThrowsAny<ArgumentException>(() => r.RegistrarLlegada(nombre, personas, TipoCliente.General));
        Assert.Equal(0, r.CantidadEnEspera);
    }

    [Fact] // CP-19
    public void Asignar_mesa_sin_clientes_lanza_excepcion()
    {
        Assert.Throws<InvalidOperationException>(() => new Restaurante().AsignarMesa());
    }
}

public class TorreControlTests
{
    [Fact] // CP-20
    public void Emergencia_obtiene_la_pista_antes_que_todos()
    {
        var t = new TorreControl();
        t.SolicitarPista("Y4521", "Volaris", CategoriaVuelo.Despegue);
        t.SolicitarPista("AM123", "Aeroméxico", CategoriaVuelo.Aterrizaje);
        t.SolicitarPista("AA900", "American", CategoriaVuelo.CombustibleBajo);
        t.SolicitarPista("UA77", "United", CategoriaVuelo.Emergencia);

        string[] orden = Enumerable.Range(0, 4).Select(_ => t.AutorizarSiguiente().Codigo).ToArray();
        Assert.Equal(new[] { "UA77", "AA900", "AM123", "Y4521" }, orden);
    }

    [Fact] // CP-21
    public void No_se_permite_el_mismo_vuelo_dos_veces_en_la_cola()
    {
        var t = new TorreControl();
        t.SolicitarPista("am123", "Aeroméxico", CategoriaVuelo.Aterrizaje); // se normaliza a AM123
        Assert.Throws<InvalidOperationException>(() =>
            t.SolicitarPista("AM123", "Aeroméxico", CategoriaVuelo.Aterrizaje));
    }

    [Fact] // CP-22
    public void Codigo_de_vuelo_con_formato_invalido_es_rechazado()
    {
        var t = new TorreControl();
        Assert.Throws<ArgumentException>(() => t.SolicitarPista("VUELO-1", "X", CategoriaVuelo.Despegue));
    }
}
