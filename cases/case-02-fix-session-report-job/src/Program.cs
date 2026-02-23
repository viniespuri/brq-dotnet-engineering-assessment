using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

internal class Program
{
    private static void Main(string[] args)
    {
        var timeBuckets = GetTimeBucketsDictionary();

        var filePath = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])
            ? args[0]
            : @"C:\temp\fix_session.summary";

        var outputPath = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1])
            ? args[1]
            : @"C:\temp\output.csv";

        Directory.CreateDirectory(@"C:\temp");

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Arquivo não encontrado: {filePath}");
            return;
        }

        var lines = File.ReadAllLines(filePath)
            .Where(msg => msg.StartsWith("IN") && msg.Contains("35=D"))
            .Select(msg => msg.Substring(13, 8))
            .GroupBy(t => t)
            .Select(x => (chave: x.Key, valor: x.Count()));

        foreach (var line in lines)
        {
            timeBuckets[line.chave] = line.valor;
        }

        SaveCSV(timeBuckets, outputPath);
        Console.WriteLine($"CSV gerado em: {outputPath}");
    }

    private static Dictionary<string, int> GetTimeBucketsDictionary()
    {
        var dict = new Dictionary<string, int>();

        var startingDate = new DateTime(2021, 02, 22, 10, 00, 00);
        var targetTime = new DateTime(2021, 02, 22, 21, 00, 00);

        for (DateTime date = startingDate; date <= targetTime; date = date.AddSeconds(1))
            dict.Add(date.TimeOfDay.ToString(), 0);

        return dict;
    }

    private static void SaveCSV(Dictionary<string, int> dict, string outputFilePath)
    {
        var file = dict.Select(x => $"{x.Key},{x.Value}").ToArray();
        File.WriteAllLines(outputFilePath, file);
    }
}
