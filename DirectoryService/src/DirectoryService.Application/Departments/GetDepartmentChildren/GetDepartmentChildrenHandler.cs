using CSharpFunctionalExtensions;
using Dapper;
using Shared.Core.Database;
using Shared.Core.Validation;
using DirectoryService.Contracts.Departments.GetChildren;
using DirectoryService.Contracts.Dtos;
using DirectoryService.Domain;
using FluentValidation;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Core.Caching;
using Shared.Kernel.Errors;

namespace DirectoryService.Application.Departments.GetDepartmentChildren;

public class GetDepartmentChildrenHandler
{
    private readonly IValidator<GetDepartmentChildrenQuery> _validator;
    private readonly IConnectionFactory _connectionFactory;
    private readonly ICacheService _cacheService;
    public GetDepartmentChildrenHandler(
        IValidator<GetDepartmentChildrenQuery> validator,
        IConnectionFactory connectionFactory,
        ICacheService cacheService)
    {
        _validator = validator;
        _connectionFactory = connectionFactory;
        _cacheService = cacheService;
    }

    public async Task<Result<GetDepartmentChildrenResponse, Errors>> HandleAsync(
        GetDepartmentChildrenQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToAppErrors();

        // Достаём из кэша
        var cacheKey = Constants.DepartmentConstants.CachePrefix
                       + "getDepartmentChildren?"
                       + $"page={query.Page}"
                       + $"pageSize={query.PageSize}"
                       + $"parentId={query.ParentId}";
        var cachedDepartments =
            await _cacheService.GetAsync<List<DepartmentWithChildrenDto>>(cacheKey, cancellationToken);
        if (cachedDepartments is not null)
            return new GetDepartmentChildrenResponse() { Departments = cachedDepartments };
        
        var connection = await _connectionFactory.CreateConnectionAsync();

        const string sql =
            """
                SELECT  d.id, 
                        d.parent_id,
                        d.name,
                        d.identifier,
                        d.path,
                        d.depth,
                        d.is_active,
                        d.created_at,
                        d.updated_at,
                       (EXISTS(SELECT 1 FROM departments WHERE parent_id = d.id)) as has_more_children
                       FROM departments d WHERE parent_id = @ParentId LIMIT @Limit OFFSET @Offset
            """;

        var result = await connection.QueryAsync<DepartmentWithChildrenDto>(sql, new
        {
            ParentId = query.ParentId,
            Limit = query.PageSize,
            Offset = (query.Page - 1) *  query.PageSize
        });
        var listedResult = result.ToList();
        
        // Кладём в кэш
        await _cacheService.SetAsync(
            cacheKey,
            listedResult,
            new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            },
            cancellationToken);

        return new GetDepartmentChildrenResponse() { Departments = listedResult.ToList() };
    }
}