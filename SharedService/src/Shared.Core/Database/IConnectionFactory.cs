using System.Data;

namespace Shared.Core.Database;

public interface IConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
}