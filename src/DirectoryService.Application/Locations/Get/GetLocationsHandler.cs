using System.Text;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Extensions;
using DirectoryService.Contracts.Locations.Get;
using FluentValidation;
using Shared.Errors;
using Dapper;
using DirectoryService.Contracts.Dtos;
using DirectoryService.Contracts.Sorting;

namespace DirectoryService.Application.Locations.Get;

public class GetLocationsHandler
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly IValidator<GetLocationsQuery> _validator;
    public GetLocationsHandler(IConnectionFactory connectionFactory, IValidator<GetLocationsQuery> validator)
    {
        _connectionFactory = connectionFactory;
        _validator = validator;
    }

    public async Task<Result<GetLocationsResponse, Errors>> HandleAsync(GetLocationsQuery query,
        CancellationToken cancellationToken)
    {
        // Провалидируем page и pageSize
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToAppErrors();
        
        var connection = await _connectionFactory.CreateConnectionAsync();

        var sqlStringBuilder = new StringBuilder();
        var parameters = new DynamicParameters();
        // base sql query
        sqlStringBuilder.Append("""
                                SELECT 
                                    l.id,
                                    l.name,
                                    l.timezone,
                                    l.created_at as created_utc,
                                    COUNT(*) OVER() AS total_count
                                FROM locations l
                                """);
        
        // add join if need
        if (query.Ids.Count > 0)
        {
            sqlStringBuilder.Append(
                "\nJOIN department_locations dl ON dl.department_id IN @DepartmentIds AND l.id = dl.location_id");
            parameters.Add("DepartmentIds", query.Ids);
        }

        // add base where clause
        sqlStringBuilder.Append("\nWHERE true");
        
        // add isActive where clause

        sqlStringBuilder.Append("\nAND l.is_active = @IsActive");
        parameters.Add("IsActive", query.IsActive);

        // add optional 'search' where clause
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            sqlStringBuilder.Append("\nAND l.name ILIKE '%' || @Search || '%'");
            parameters.Add("Search", query.Search);
        }

        var orderClauses = new List<string>();
        
        // add sorting if need
        if (query.SortOptions?.Count > 0)
            foreach (var sortOption in query.SortOptions)
            {
                if (SortableFields.Location.TryGetValue(sortOption.Field.ToLower(), out var dbFieldName))
                {
                    var direction = sortOption.Direction switch
                    {
                        SortDirection.Asc => "ASC",
                        SortDirection.Desc => "DESC",
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    orderClauses.Add($"{dbFieldName} {direction}");
                }
            }

        if (orderClauses.Count > 0)
        {
            sqlStringBuilder.Append($"\nORDER BY {string.Join(", ", orderClauses)}");
        }
        
        sqlStringBuilder.Append("\nLIMIT @PageSize OFFSET @Offset");
        parameters.Add("PageSize", query.PageSize);
        parameters.Add("Offset", (query.Page - 1) * query.PageSize);
        
        var sql = sqlStringBuilder.ToString();
        long total = 0;
        bool assigned = false;
        var locations = connection.Query<LocationDto, long, LocationDto>(
            sql, 
            (location, totalCount) =>
        {
            if (!assigned)
            {
                total = totalCount;
                assigned = true;
            }
            return location;
        }, 
            splitOn: "total_count",
            param: parameters);
        

        return new GetLocationsResponse() {  Locations = locations.ToList(), TotalCount = total };
    }
}