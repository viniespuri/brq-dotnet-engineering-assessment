# Case 4 - Superhero Class

## Identified requirements
- The class must represent a Superhero with fields `Nome`, `DataNascimento`, and `NivelKryptonita`.
- The Superhero can only fly when `NivelKryptonita < 2`.
- When able to fly, the return must be exactly `"Voando..."`.

## Suggested fix
- Model the entity with well-defined properties and constructor to guarantee valid state.
- Centralize the business rule in a `Voar()` method.
- If Kryptonite level is less than 2, return `"Voando..."`; otherwise return a blocked message.

```csharp
public sealed class SuperHeroi
{
    public string Nome { get; }
    public DateTime DataNascimento { get; }
    public int NivelKryptonita { get; private set; }

    public SuperHeroi(string nome, DateTime dataNascimento, int nivelKryptonita)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome e obrigatorio.", nameof(nome));

        if (nivelKryptonita < 0)
            throw new ArgumentOutOfRangeException(nameof(nivelKryptonita), "Nivel de Kryptonita nao pode ser negativo.");

        Nome = nome;
        DataNascimento = dataNascimento;
        NivelKryptonita = nivelKryptonita;
    }

    public string Voar()
    {
        return NivelKryptonita < 2
            ? "Voando..."
            : "Nao pode voar.";
    }
}
```

## Objective conclusion
The implementation matches the prompt with a clear flight rule (`NivelKryptonita < 2`) and encapsulates behavior in `Voar()`, keeping the class simple, testable, and compliant with the functional requirement.
