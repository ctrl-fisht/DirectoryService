using Dapper;
using Shared.Core.Database;
using DirectoryService.Contracts.Departments.GetTopPositions;
using DirectoryService.Contracts.Dtos;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.VisualBasic;
using Shared.Core.Caching;
using Constants = DirectoryService.Domain.Constants;

namespace DirectoryService.Application.Departments.GetTopPositions;

public class GetTopPositionsHandler
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ICacheService _cacheService;

    private const string CacheKey = Constants.DepartmentConstants.CachePrefix + "top5";
    public GetTopPositionsHandler(IConnectionFactory connectionFactory, ICacheService cacheService)
    {
        _connectionFactory = connectionFactory;
        _cacheService = cacheService;
    }

    public async Task<GetTopPositionsResponse> HandleAsync(CancellationToken cancellationToken)
    {
        // Достаём с кэша
        var topDepartmentsCached = 
            await _cacheService.GetAsync<List<DepartmentWithPositionsDto>>(CacheKey, cancellationToken);
        if (topDepartmentsCached is not null)
            return new GetTopPositionsResponse(){ DepartmentWithPositions = topDepartmentsCached};
        
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
        
        var result = await connection
            .QueryAsync<DepartmentWithPositionsDto>(sql);
        var listedResult = result.ToList();

        // Кладём в кэш
        await _cacheService.SetAsync(
            CacheKey,
            listedResult,
            new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            },
            cancellationToken);
        
        return new GetTopPositionsResponse(){ DepartmentWithPositions = listedResult};
    }
}