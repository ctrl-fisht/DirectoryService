using CSharpFunctionalExtensions;
using Shared.Kernel.Errors;

namespace Shared.Core.Database;

public interface ITransactionScope
{
    UnitResult<Error> Commit(CancellationToken cancellationToken);
    UnitResult<Error> Rollback(CancellationToken cancellationToken);
}