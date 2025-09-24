using System.Data;

namespace DirectoryService.Application.Database;

public interface IConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
}