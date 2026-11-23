using Npgsql;
using System.Data;

namespace RotaDoSabor.Server.Repositories;

public abstract class BaseRepository
{
    protected readonly NpgsqlConnection Connection;

    protected BaseRepository(IDbConnection connection)
    {
        Connection = (NpgsqlConnection)connection ?? throw new ArgumentNullException(nameof(connection));
    }

    protected async Task EnsureOpenAsync()
    {
        if (Connection.State != ConnectionState.Open) await Connection.OpenAsync();
    }
}
