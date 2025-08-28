using CSharpFunctionalExtensions;
using Shared.Errors;

namespace DirectoryService.Application.Database;

public interface ITransactionManager
{
    public Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken);
    public Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
}