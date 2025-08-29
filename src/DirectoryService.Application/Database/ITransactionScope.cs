using CSharpFunctionalExtensions;
using Shared.Errors;

namespace DirectoryService.Application.Database;

public interface ITransactionScope
{
    UnitResult<Error> Commit(CancellationToken cancellationToken);
    UnitResult<Error> Rollback(CancellationToken cancellationToken);
}