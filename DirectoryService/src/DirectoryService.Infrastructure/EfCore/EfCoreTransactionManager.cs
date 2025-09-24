using CSharpFunctionalExtensions;
using Shared.Core.Database;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Errors;

namespace DirectoryService.Infrastructure.EfCore;

public class EfCoreTransactionManager : ITransactionManager
{
    private readonly AppDbContext _dbContext;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ITransactionManager> _logger;

    
    public EfCoreTransactionManager(
        AppDbContext dbContext,
        ILoggerFactory loggerFactory,
        ILogger<ITransactionManager> logger)
    {
        _dbContext = dbContext;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }
    
    public async Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var loggerForTransaction = _loggerFactory.CreateLogger<TransactionScope>();
            var transactionScope = new TransactionScope(transaction.GetDbTransaction(), loggerForTransaction);
            return transactionScope;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return AppErrors.Database.ErrorWhileBeginTransaction();
        }
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
            _logger.LogError(ex, "Error while saving changes");
            return AppErrors.Database.ErrorWhileSavingChanges();
        }
    }
}