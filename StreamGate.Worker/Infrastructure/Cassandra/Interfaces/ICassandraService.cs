using Cassandra;

namespace StreamGate.Worker.Infrastructure.Cassandra.Interfaces;

public interface ICassandraService
{
    ISession GetSession();
}