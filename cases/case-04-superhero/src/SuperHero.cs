namespace Case04.Superhero;

public sealed class SuperHero
{
    public string Name { get; }
    public DateTime DateOfBirth { get; }
    public int KryptoniteLevel { get; private set; }

    public SuperHero(string name, DateTime dateOfBirth, int kryptoniteLevel)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Nome é Obrigatório.", nameof(name));
        }

        if (kryptoniteLevel < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(kryptoniteLevel), "Kriptonita não pode ser negativa.");
        }

        Name = name;
        DateOfBirth = dateOfBirth;
        KryptoniteLevel = kryptoniteLevel;
    }

    public string Fly()
    {
        return KryptoniteLevel < 2 ? "Voando..." : "Não pode voar.";
    }
}
