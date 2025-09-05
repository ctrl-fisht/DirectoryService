using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Application.Extensions;
using DirectoryService.Contracts.Departments.GetRootsWithChildren;
using DirectoryService.Contracts.Dtos;
using FluentValidation;
using Shared.Errors;

namespace DirectoryService.Application.Departments.GetRootsWithChildren;

public class GetRootsWithChildrenHandler
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly IValidator<GetRootsWithChildrenQuery> _validator;
    
    public GetRootsWithChildrenHandler(
        IConnectionFactory connectionFactory,
        IValidator<GetRootsWithChildrenQuery> validator)
    {
        _connectionFactory = connectionFactory;
        _validator = validator;
    }

    public async Task<Result<GetRootsWithChildrenResponse, Errors>> HandleAsync(
        GetRootsWithChildrenQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToAppErrors();

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

        return new GetRootsWithChildrenResponse()
        {
            Roots = roots
        };
    }
}