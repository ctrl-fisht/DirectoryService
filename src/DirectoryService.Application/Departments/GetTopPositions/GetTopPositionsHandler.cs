using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments.GetTopPositions;
using DirectoryService.Contracts.Dtos;

namespace DirectoryService.Application.Departments.GetTopPositions;

public class GetTopPositionsHandler
{
    private readonly IConnectionFactory _connectionFactory;
    
    public GetTopPositionsHandler(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<GetTopPositionsResponse> HandleAsync(CancellationToken token)
    {
        var connection = await _connectionFactory.CreateConnectionAsync();

        const string sql =
            """
            WITH dp AS (
                SELECT 
                    department_id,
                    COUNT(*) as positions_count
                FROM department_positions
                GROUP BY department_id
            )
            SELECT d.id, d.name, d.identifier, d.path, d.depth, d.created_at, dp.positions_count 
            FROM departments d
            JOIN dp ON d.id = dp.department_id
            ORDER BY dp.positions_count DESC
            LIMIT 5; 
            """;
        var result = await connection.QueryAsync<DepartmentWithPositionsDto>(sql);
        
        return new GetTopPositionsResponse(){ DepartmentWithPositions = result.ToList()};
    }
}