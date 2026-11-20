using System.Numerics;
using EstructurasLineales.Core.Estructuras;

namespace EstructurasLineales.Core.Llamadas;

/// <summary>Un registro de activación (stack frame) de la pila de llamadas.</summary>
public record MarcoLlamada(string Funcion, int Argumento)
{
    public override string ToString() => $"{Funcion}({Argumento})";
}

public enum TipoPaso
{
    Llamada,        // PUSH de un marco
    Retorno,        // POP de un marco
    Desbordamiento  // Stack Overflow simulado
}

/// <summary>Fotografía de la pila de llamadas después de una operación.</summary>
/// <param name="PilaActual">Marcos de la pila, del tope al fondo.</param>
public record PasoLlamada(TipoPaso Tipo, string Descripcion, IReadOnlyList<string> PilaActual, BigInteger? Resultado = null);

/// <summary>
/// Simula cómo el sistema usa una PILA para gestionar llamadas recursivas.
/// Ejemplo: factorial(n) = n * factorial(n - 1), con caso base factorial(0) = factorial(1) = 1.
/// </summary>
public class SimuladorPilaLlamadas
{
    /// <summary>Número máximo de marcos antes de producir un "Stack Overflow".</summary>
    public int LimiteProfundidad { get; }

    public SimuladorPilaLlamadas(int limiteProfundidad = 20)
    {
        if (limiteProfundidad < 1)
            throw new ArgumentOutOfRangeException(nameof(limiteProfundidad), "El límite debe ser al menos 1.");
        LimiteProfundidad = limiteProfundidad;
    }

    /// <summary>Genera, paso a paso, las operaciones PUSH/POP que provoca factorial(n).</summary>
    public IReadOnlyList<PasoLlamada> SimularFactorial(int n)
    {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n), "n debe ser mayor o igual que 0.");

        var pila = new Pila<MarcoLlamada>();
        var pasos = new List<PasoLlamada>();

        // 1) Fase de llamadas: cada llamada recursiva apila un marco nuevo.
        int k = n;
        while (true)
        {
            if (pila.Cantidad >= LimiteProfundidad)
            {
                pasos.Add(new PasoLlamada(TipoPaso.Desbordamiento,
                    $"STACK OVERFLOW: se superó el límite de {LimiteProfundidad} marcos",
                    Instantanea(pila)));
                return pasos;
            }

            pila.Push(new MarcoLlamada("factorial", k));
            pasos.Add(new PasoLlamada(TipoPaso.Llamada, $"PUSH  → llamada a factorial({k})", Instantanea(pila)));

            if (k <= 1) break; // Caso base: ya no hay más llamadas.
            k--;
        }

        // 2) Fase de retornos: se desapilan los marcos en orden inverso (LIFO).
        BigInteger resultado = BigInteger.One;
        while (!pila.EstaVacia)
        {
            MarcoLlamada marco = pila.Pop();
            resultado = marco.Argumento <= 1 ? BigInteger.One : marco.Argumento * resultado;
            pasos.Add(new PasoLlamada(TipoPaso.Retorno,
                $"POP   ← factorial({marco.Argumento}) retorna {resultado}",
                Instantanea(pila), resultado));
        }

        return pasos;
    }

    private static IReadOnlyList<string> Instantanea(Pila<MarcoLlamada> pila) =>
        pila.Select(m => m.ToString()).ToList();
}
