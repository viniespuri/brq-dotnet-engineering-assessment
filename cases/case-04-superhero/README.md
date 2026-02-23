# Case 4 - Classe Super-Heroi

## Requisitos identificados
- A classe deve representar um Super-heroi com os campos `Nome`, `DataNascimento` e `NivelKryptonita`.
- O Super-heroi so pode voar quando `NivelKryptonita < 2`.
- Quando estiver apto a voar, o retorno deve ser exatamente `"Voando..."`.

## Correcao sugerida
- Modelar a entidade com propriedades bem definidas e construtor para garantir estado valido.
- Centralizar a regra de negocio em um metodo `Voar()`.
- Se o nivel de Kryptonita for menor que 2, retornar `"Voando..."`; caso contrario, retornar uma mensagem de bloqueio.

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

## Conclusao objetiva
A implementacao atende ao enunciado com uma regra clara de voo (`NivelKryptonita < 2`) e encapsula o comportamento no metodo `Voar()`, mantendo a classe simples, testavel e aderente ao requisito funcional.
