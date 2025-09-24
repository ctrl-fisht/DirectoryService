using CSharpFunctionalExtensions;
using Shared.Kernel.Errors;

namespace Shared.Core.Database;

public interface ITransactionManager
{
    public Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken);
    public Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
}