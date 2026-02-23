using Case04.Superhero;

var heroes = new List<SuperHero>
{
    new("Superman", new DateTime(1938, 6, 1), 1),
    new("Mr. Incredible", new DateTime(1958, 10, 1), 3),
    new("Mr. Beast", new DateTime(1985, 10, 1), 0)
};

heroes.ForEach(hero =>
{
    Console.WriteLine($"Hero: {hero.Name}");
    Console.WriteLine($"Date of Birth: {hero.DateOfBirth:yyyy-MM-dd}");
    Console.WriteLine($"Kryptonite Level: {hero.KryptoniteLevel}");
    Console.WriteLine($"Fly result: {hero.Fly()}");
    Console.WriteLine();
});
