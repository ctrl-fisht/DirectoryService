using CSharpFunctionalExtensions;
using DirectoryService.Application.Repositories;
using DirectoryService.Domain.Entities;
using DirectoryService.Infrastructure.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Dapper;
using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Infrastructure.Repositories;

public class DepartmentsRepository : IDepartmentsRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DepartmentsRepository> _logger;
    
    public DepartmentsRepository(AppDbContext dbContext, ILogger<DepartmentsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Departments.AddAsync(department, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return department.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while adding department");
            return AppErrors.Database.ErrorWhileAdding("department");
        }
    }

    public async Task<Result<Department, Error>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        // предполагаю что Department почти всегда будет использоваться с locations и positions
        var result = await _dbContext.Departments
            .Include(d => d.DepartmentLocations)
            .Include(d => d.DepartmentPositions)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (result == null)
            return AppErrors.General.NotFound(id.ToString());
        return result;
    }

    public async Task<bool> IsIdentifierExistAsync(string identifier, CancellationToken cancellationToken)
    {
        return  await _dbContext.Departments.AnyAsync(d => d.Identifier.Value == identifier);
    }

    public async Task<bool> AllExistsAsync(List<Guid> ids, CancellationToken cancellationToken)
    {
        var count = await _dbContext.Departments
            .Where(d => ids.Contains(d.Id))
            .Where(d => d.IsActive)
            .CountAsync(cancellationToken);
        
        return count == ids.Count;
    }

    public async Task<Result<Department, Error>> GetByIdWithLockAsync(Guid id, CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments
            .FromSqlInterpolated($"SELECT * FROM departments WHERE id = {id} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);
        
        if (department == null)
            return AppErrors.General.NotFound(id.ToString());
        return department;
    }

    public async Task<UnitResult<Error>> LockDescendantsAsync(Department department, CancellationToken cancellationToken)
    {
        var conn = _dbContext.Database.GetDbConnection();

        const string sql =
            """
            SELECT id 
            FROM departments 
            WHERE path <@ @rootPath::ltree
            AND id != @selfId
            FOR UPDATE
            """;
        
        await conn.ExecuteAsync(sql, new
        {
            rootPath = department.Path.Value,
            selfId = department.Id
        });

        return UnitResult.Success<Error>();
    }

    public async Task<bool> IsDescendantAsync(DepartmentPath rootPath, Guid candidateForCheckId, CancellationToken cancellationToken)
    {
        var conn = _dbContext.Database.GetDbConnection();

        const string sql =
            """
            SELECT 1 
            FROM departments 
            WHERE path <@ @rootPath::ltree
            AND id = @candidateId
            """;
        
        var result = await conn.QueryFirstOrDefaultAsync<int?>(sql, new
        {
            rootPath = rootPath.Value,
            candidateId = candidateForCheckId
        });

        return result.HasValue;
    }

    public async Task<UnitResult<Error>> MoveDepartmentAsync(
        Department departmentUpdated,
        DepartmentPath oldPath, CancellationToken cancellationToken)
    {
        var conn = _dbContext.Database.GetDbConnection();

        const string sql =
            """
            UPDATE departments
            SET
                parent_id = CASE 
                                WHEN id = @id THEN @parentId 
                                ELSE parent_id 
                            END,
                path = CASE 
                            WHEN id = @id THEN @newPath::ltree
                            ELSE @newPath::ltree || subpath(path, nlevel(@oldPath::ltree))
                       END,
                depth = CASE
                            WHEN id = @id THEN @newDepth
                            ELSE @newDepth + (depth - nlevel(@oldPath::ltree))
                        END
            WHERE path <@ @oldPath::ltree
            """;
        
        await conn.ExecuteAsync(sql, new
        {
            id = departmentUpdated.Id,
            parentId = departmentUpdated.ParentId,
            newPath = departmentUpdated.Path.Value,
            oldPath = oldPath.Value,
            newDepth = departmentUpdated.Depth
        });

        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while saving department changes");
            return AppErrors.Database.ErrorWhileSavingChanges();
        }
    }
}