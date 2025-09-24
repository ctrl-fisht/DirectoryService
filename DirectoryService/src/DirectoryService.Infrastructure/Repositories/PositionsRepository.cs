using CSharpFunctionalExtensions;
using DirectoryService.Application.Repositories;
using DirectoryService.Domain.Entities;
using DirectoryService.Infrastructure.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Errors;

namespace DirectoryService.Infrastructure.Repositories;

public class PositionsRepository : IPositionsRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<PositionsRepository> _logger;
    
    public PositionsRepository(AppDbContext dbContext, ILogger<PositionsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> CreateAsync(Position position, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Positions.AddAsync(position, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return position.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating location");
            return AppErrors.Database.ErrorWhileAdding("position");
        }
    }

    public async Task<bool> ExistByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _dbContext.Positions.AnyAsync(p => p.Name == name, cancellationToken);
    }
}