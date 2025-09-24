using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Application.Extensions;
using DirectoryService.Contracts.Departments.GetRootsWithChildren;
using DirectoryService.Contracts.Dtos;
using FluentValidation;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.VisualBasic;
using Shared.Caching;
using Shared.Errors;
using Constants = DirectoryService.Domain.Constants;

namespace DirectoryService.Application.Departments.GetRootsWithChildren;

public class GetRootsWithChildrenHandler
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly IValidator<GetRootsWithChildrenQuery> _validator;
    private readonly ICacheService _cacheService;
    
    public GetRootsWithChildrenHandler(
        IConnectionFactory connectionFactory,
        IValidator<GetRootsWithChildrenQuery> validator,
        ICacheService cacheService)
    {
        _connectionFactory = connectionFactory;
        _validator = validator;
        _cacheService = cacheService;
    }

    public async Task<Result<GetRootsWithChildrenResponse, Errors>> HandleAsync(
        GetRootsWithChildrenQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToAppErrors();

        // Достаём с кэша
        var cacheKey =
            Constants.DepartmentConstants.CachePrefix
            + "getRootsWithChildren?"
            + $"page={query.Page}"
            + $"pageSize={query.PageSize}"
            + $"prefetch={query.Prefetch}";
        var cachedDepartments =
            await _cacheService.GetAsync<List<DepartmentWithChildrenDto>>(cacheKey, cancellationToken);
        if (cachedDepartments is not null)
            return new GetRootsWithChildrenResponse() { Roots = cachedDepartments };
        
        var connection = await _connectionFactory.CreateConnectionAsync();

        const string sql =
            """
            WITH roots AS (
                SELECT 
                    d.id, 
                    d.parent_id,
                    d.name,
                    d.identifier,
                    d.path,
                    d.depth,
                    d.is_active,
                    d.created_at,
                    d.updated_at 
                FROM departments d
                WHERE parent_id IS NULL
                ORDER BY d.created_at
                LIMIT @RootsLimit OFFSET @RootsOffset
            )
            SELECT *, (EXISTS(SELECT 1 FROM departments d WHERE d.parent_id = roots.id OFFSET @ChildrenLimit LIMIT 1)) as has_more_children 
            FROM roots
                     
            UNION ALL

            SELECT c.*, 
                   (EXISTS(SELECT 1 FROM departments d WHERE d.parent_id = c.id)) as has_more_children
            FROM roots
            CROSS JOIN LATERAL (
                SELECT d.id,
                       d.parent_id,
                       d.name,
                       d.identifier,
                       d.path,
                       d.depth,
                       d.is_active,
                       d.created_at,
                       d.updated_at
                FROM departments d
                WHERE d.parent_id = roots.id
                LIMIT @ChildrenLimit
                ) c;

            """;

        var flatResult = await connection.QueryAsync<DepartmentWithChildrenDto>(
            sql,
            new
            {
                RootsLimit = query.PageSize,
                RootsOffset = (query.Page - 1) * query.PageSize,
                ChildrenLimit = query.Prefetch,
            });

        var departmentWithChildrenDtos = flatResult.ToList();
        
        var roots = departmentWithChildrenDtos
            .Where(dep => dep.ParentId == null)
            .ToList();
        
        var childrenDict = departmentWithChildrenDtos
            .Where(dep => dep.ParentId != null)
            .GroupBy(dep => dep.ParentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var root in roots)
        {
            if (childrenDict.TryGetValue(root.Id, out var children))
                root.Children.AddRange(children);
        }
        
        // Кладём в кэш
        await _cacheService.SetAsync(
            cacheKey,
            roots,
            new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            },
            cancellationToken);
        
        return new GetRootsWithChildrenResponse()
        {
            Roots = roots
        };
    }
}