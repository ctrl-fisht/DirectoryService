using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Application.Extensions;
using DirectoryService.Contracts.Departments.GetChildren;
using DirectoryService.Contracts.Dtos;
using FluentValidation;
using Shared.Errors;

namespace DirectoryService.Application.Departments.GetDepartmentChildren;

public class GetDepartmentChildrenHandler
{
    private readonly IValidator<GetDepartmentChildrenQuery> _validator;
    private readonly IConnectionFactory _connectionFactory;
    
    public GetDepartmentChildrenHandler(
        IValidator<GetDepartmentChildrenQuery> validator,
        IConnectionFactory connectionFactory)
    {
        _validator = validator;
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<GetDepartmentChildrenResponse, Errors>> HandleAsync(
        GetDepartmentChildrenQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToAppErrors();

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


        return new GetDepartmentChildrenResponse() { Departments = result.ToList() };
    }
}