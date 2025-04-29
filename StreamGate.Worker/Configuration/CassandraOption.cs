namespace StreamGate.Worker.Configuration;

public class CassandraOption
{
    public const string Section = "Cassandra";
    public string ContactPoint { get; set; } = string.Empty;
}