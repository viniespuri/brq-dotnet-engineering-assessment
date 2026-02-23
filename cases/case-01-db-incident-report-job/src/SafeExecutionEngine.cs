using Microsoft.Data.SqlClient;

public sealed class SafeExecutionEngine : IRuleEngine
{
    private const string DefaultConnectionString = "Server=localhost,1433;Database=case01db;User Id=sa;Password=BrqTest!Passw0rd;TrustServerCertificate=True;Encrypt=False;";
    private readonly string _connectionString;
    private readonly int _pollIntervalMs;
    private readonly int _batchSize;
    private readonly string _outputDirectory;
    private readonly string _checkpointFilePath;
    private readonly int _maxOutputFiles;
    private readonly object _sync = new();
    private DateTime _lastCycleUtc = DateTime.MinValue;
    private DateTime _checkpointUtc;

    /// <summary>
    /// Inicializo a engine lendo configuracoes externas, limites operacionais e checkpoint persistido, 
    /// uso variaveis de ambiente para reduzir acoplamento com o codigo e permitir ajuste sem recompilar, 
    /// e carrego o checkpoint para retomar do ultimo ponto sem reprocessar eventos apos reinicio.
    /// </summary>
    public SafeExecutionEngine()
    {
        _connectionString = ReadString("HOTFIX_DB_CONN", DefaultConnectionString);
        _pollIntervalMs = ReadInt("HOTFIX_POLL_INTERVAL_MS", 2000, 250, 60000);
        _batchSize = ReadInt("HOTFIX_BATCH_SIZE", 200, 1, 5000);
        _outputDirectory = ReadString("HOTFIX_OUTPUT_DIR", @"C:\temp");
        _checkpointFilePath = ReadString("HOTFIX_CHECKPOINT_FILE", Path.Combine(AppContext.BaseDirectory, ".eventos.checkpoint"));
        _maxOutputFiles = ReadInt("HOTFIX_MAX_OUTPUT_FILES", 30, 1, 1000);
        Directory.CreateDirectory(_outputDirectory);
        _checkpointUtc = LoadCheckpoint(_checkpointFilePath);
    }

    /// <summary>
    /// Executo um ciclo incremental com controle de frequencia, tamanho de lote e atualizacao de checkpoint, 
    /// uso lock para impedir concorrencia sobre o mesmo estado interno e mantenho consulta parametrizada com descarte correto de recursos 
    /// para reforcar seguranca e estabilidade.
    /// </summary>
    public void Execute()
    {
        lock (_sync)
        {
            Throttle();
            var cycleStartUtc = DateTime.UtcNow;
            var eventos = new List<string>(_batchSize);
            var maxTimestampUtc = _checkpointUtc;

            // Eu congelo o limite superior da janela no inicio do ciclo para ter leitura consistente.
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = @"SELECT TOP (@BatchSize) [timestamp], [evento]
                                    FROM [eventos]
                                    WHERE [timestamp] > @Checkpoint AND [timestamp] <= @CycleStart
                                    ORDER BY [timestamp] ASC;";

                cmd.Parameters.Add(new SqlParameter("@BatchSize", System.Data.SqlDbType.Int) { Value = _batchSize });
                cmd.Parameters.Add(new SqlParameter("@Checkpoint", System.Data.SqlDbType.DateTime2) { Value = _checkpointUtc });
                cmd.Parameters.Add(new SqlParameter("@CycleStart", System.Data.SqlDbType.DateTime2) { Value = cycleStartUtc });

                // Eu uso parametros para evitar SQL injection e para preservar tipos corretos no banco.
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var ts = reader.GetDateTime(0).ToUniversalTime();
                        if (ts > maxTimestampUtc)
                        {
                            maxTimestampUtc = ts;
                        }

                        var texto = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        eventos.Add($"{ts:O}|{texto}");
                    }
                }
            }

            // Eu sempre persisto checkpoint no fim do ciclo para manter progresso mesmo sem novos eventos.
            if (eventos.Count > 0)
            {
                var outputFile = Path.Combine(_outputDirectory, $"eventos_{DateTime.UtcNow:yyyyMMdd}.log");
                File.AppendAllLines(outputFile, eventos);
                RotateOutputFiles();
                _checkpointUtc = maxTimestampUtc;
                SaveCheckpoint(_checkpointFilePath, _checkpointUtc);
            }
            else
            {
                _checkpointUtc = cycleStartUtc;
                SaveCheckpoint(_checkpointFilePath, _checkpointUtc);
            }

            _lastCycleUtc = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Aplico atraso entre ciclos com base no ultimo horario de execucao e no intervalo configurado para evitar loop agressivo que satura CPU e banco,
    /// reduzindo pressao no pool de conexoes e estabilizando a taxa de processamento.
    /// </summary>
    private void Throttle()
    {
        var now = DateTime.UtcNow;
        if (_lastCycleUtc == DateTime.MinValue)
        {
            return;
        }

        var elapsedMs = (int)(now - _lastCycleUtc).TotalMilliseconds;
        var delayMs = _pollIntervalMs - elapsedMs;
        if (delayMs > 0)
        {
            Thread.Sleep(delayMs);
        }
    }

    /// <summary>
    /// Mantenho somente o numero maximo de arquivos de saida permitido e removo os mais antigos para evitar crescimento descontrolado de disco e 
    /// reduzir risco de indisponibilidade por excesso de I/O.
    /// </summary>
    private void RotateOutputFiles()
    {
        var files = new DirectoryInfo(_outputDirectory)
            .GetFiles("eventos_*.log", SearchOption.TopDirectoryOnly)
            .OrderByDescending(f => f.CreationTimeUtc)
            .ToArray();

        for (var i = _maxOutputFiles; i < files.Length; i++)
        {
            files[i].Delete();
        }
    }

    /// <summary>
    /// Carrego o checkpoint persistido para retomar a partir do ultimo timestamp valido, 
    /// uso fallback de 5 minutos quando o arquivo nao existe ou esta invalido para iniciar com seguranca, 
    /// e reduzo reprocessamento para acelerar a recuperacao apos restart.
    /// </summary>
    private static DateTime LoadCheckpoint(string path)
    {
        if (!File.Exists(path))
        {
            return DateTime.UtcNow.AddMinutes(-5);
        }

        var raw = File.ReadAllText(path).Trim();
        if (DateTime.TryParse(raw, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var parsed))
        {
            return parsed.ToUniversalTime();
        }

        return DateTime.UtcNow.AddMinutes(-5);
    }

    /// <summary>
    /// Persisto em disco o ultimo checkpoint processado no formato Utc para garantir retomada consistente entre reinicializacoes e evitar duplicidade 
    /// causada por perda de estado em memoria.
    /// </summary>
    private static void SaveCheckpoint(string path, DateTime checkpointUtc)
    {
        File.WriteAllText(path, checkpointUtc.ToString("O"));
    }

    /// <summary>
    /// Leio string de variavel de ambiente com fallback seguro para tirar valor fixo do codigo, reduzir acoplamento operacional 
    /// e facilitar ajustes de ambiente sem recompilacao.
    /// </summary>
    private static string ReadString(string key, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    /// <summary>
    /// Leio numero de variavel de ambiente com validacao e limites minimo e maximo, 
    /// aplico clamp para bloquear configuracao perigosa fora da faixa permitida e protejo o runtime contra comportamento extremo em producao.
    /// </summary>
    private static int ReadInt(string key, int fallback, int min, int max)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (!int.TryParse(value, out var parsed))
        {
            return fallback;
        }

        if (parsed < min)
        {
            return min;
        }

        if (parsed > max)
        {
            return max;
        }

        return parsed;
    }
}
