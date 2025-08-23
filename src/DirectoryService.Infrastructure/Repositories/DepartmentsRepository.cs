using CSharpFunctionalExtensions;
using DirectoryService.Application.Repositories;
using DirectoryService.Domain.Entities;
using DirectoryService.Infrastructure.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Errors;

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
        var result = await _dbContext.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
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
}