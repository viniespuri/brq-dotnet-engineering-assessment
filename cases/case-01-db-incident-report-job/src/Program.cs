using Microsoft.Data.SqlClient;
internal class Program
{
    static void Main()
    {
        var svc = new Service();
        var task = Task.Run(() => svc.Run());
        Task.WaitAll(new Task[] { task });
    }
}

public class Service
{
    public void Run()
    {
        var engine = new RuleEngine();
        while (true)
        {
            engine.Execute();
        }
    }
}
public class RuleEngine : IRuleEngine
{
    public void Execute()
    {
         var connString =
            Environment.GetEnvironmentVariable("DB_CONNECTION")
            ?? "Server=localhost,1433;Database=case01db;User Id=sa;Password=BrqTest!Passw0rd;TrustServerCertificate=True;Encrypt=False;";
        var query = $"SELECT evento FROM eventos WHERE timestamp > '{DateTime.Now.AddMinutes(-5):yyyy-MM-dd hh:mm:ss}'";
        var conn = new SqlConnection(connString);
        var cmd = new SqlCommand(query, conn);
        conn.Open();
        var reader = cmd.ExecuteReader();
        var eventos = new List<string>();
        while (reader.Read())
        {
            eventos.Add(reader[0].ToString());
        }
        File.WriteAllLines($@"C:\temp\eventos_{Guid.NewGuid()}.txt", eventos.ToArray());
    }
}
public interface IRuleEngine
{
    void Execute();
}