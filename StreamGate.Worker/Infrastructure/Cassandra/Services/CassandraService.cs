using Cassandra;
using Microsoft.Extensions.Options;
using StreamGate.Worker.Configuration; 
using StreamGate.Worker.Infrastructure.Cassandra.Interfaces;

namespace StreamGate.Worker.Infrastructure.Cassandra.Services;

public class CassandraService : ICassandraService
{
    private readonly ISession _session;
    
    public CassandraService(IOptions<CassandraOption> cassandraOption)
    {  
        Cluster cluster = Cluster.Builder()
            .AddContactPoint(cassandraOption.Value.ContactPoint).Build(); 
        _session = cluster.Connect();
        
        Task.Run(async () => 
        {
            await CreateKeySpace();
            await CreateTable();
        });
    }
    
    public ISession GetSession()
    {
        return _session;
    }
    
    private async Task CreateKeySpace()
    {
        string createKeySpaceQuery = @"CREATE KEYSPACE IF NOT EXISTS sensor_data
            WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 3};";
        
        await _session.ExecuteAsync(new SimpleStatement(createKeySpaceQuery)); 
    }

    private async Task CreateTable()
    {
        string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS sensor_data.sensor_values (
                sensor_id UUID,
                sensor_name TEXT,
                value FLOAT,
                alert_type TEXT,
                alert_message TEXT,
                created_at_utc TIMESTAMP,
                PRIMARY KEY (sensor_id, created_at_utc)
            );";
        
        await _session.ExecuteAsync(new SimpleStatement(createTableQuery)); 
    }
}