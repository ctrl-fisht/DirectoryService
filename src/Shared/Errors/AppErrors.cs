namespace Shared.Errors;

public static partial class AppErrors
{
    public static partial class Validation
    {
        public static Error CannotBeEmpty(string name)
        {
            return Error.Validation(
                "param.is.empty",
                $"Parameter '{name}' cannot be empty",
                $"{name}");
        }

        public static Error LengthNotInRange(string name, int min, int max)
        {
            return Error.Validation(
                "param.bad.length",
                $"Parameter '{name}' length must be between {min} and {max}",
                $"{name}");
        }

        public static Error BadFormat(string name, string allowed)
        {
            return Error.Validation(
                "param.bad.format",
                $"Parameter '{name}' has invalid format. Allowed: '{allowed}'",
                $"{name}");
        }

        public static Error TooLong(string name, int max)
        {
            return Error.Validation(
                "param.bad.length",
                $"Parameter '{name}' is too long. Max: '{max}' symbols",
                $"{name}");
        }

        public static Error MustBeGreaterOrEqualThan(string name, int min)
        {
            return Error.Validation(
                "param.too.small",
                $"Parameter '{name}' must be greater than or equal to '{min}'",
                $"{name}");
        }

        public static Error DuplicatesInList(string name)
        {
            return Error.Validation(
                "param.duplicate",
                $"List {name} has duplicates",
                $"{name}");
        }
    }

    public partial class General
    {
        public static Error NotFound(string id)
        {
            return Error.Validation("record.not.found", $"Record with given id='{id}' was not found");
        }

        public static Error GivenIdsNotExists(string field)
        {
            return Error.Validation("record.not.found", $"Some records in field '{field} are not exist'");
        }

        public static Error AlreadyExists(string fieldName)
        {
            return Error.Conflict("record.already.exists", $"Record with given '{fieldName}' already exists");
        }
        

        public static Error SomethingWentWrong()
        {
            return Error.Failure("something.went.wrong", "Something went wrong");
        }

        public static Error ErrorWhile(string @while)
        {
            return Error.Failure("something.went.wrong", @while);
        }
    }

    public static partial class Database
    {
        public static Error ErrorWhileAdding(string record)
        {
            return Error.Failure("database.error", $"Error while adding '{record}'");
        }
        
        public static Error ErrorWhileSavingChanges()
        {
            return Error.Failure("database.error", $"Error while saving changes");
        }

        public static Error ErrorWhileBeginTransaction()
        {
            return Error.Failure("database.error", $"Error while begin transaction");
        }

        public static Error ErrorWhileCommitTransaction()
        {
            return Error.Failure("database.error", $"Error while commit transaction");
        }

        public static Error ErrorWhileRollbackTransaction()
        {
            return Error.Failure("database.error", $"Error while rollback transaction");
        }
    }

    public partial class Hierarchy
    {
        public static Error CannotAddSelfAsAChild()
        {
            return Error.Failure("cannot.add.self", "Cannot add self as a child");
        }
        
        public static Error CannotAddSelfAsAParent()
        {
            return Error.Failure("cannot.add.self", "Cannot add self as a parent");
        }

        public static Error CannotAddAncestor()
        {
            return Error.Failure("cannot.add.ancestor", "Cannot add ancestor as a child");
        }

        public static Error ParentHasNoSuchChild(string parentId)
        {
            return Error.Failure("parent.no.child", $"Given parent '{parentId}' has no such child");
        }

        public static Error CannotAddChildAsAncestor()
        {
            return Error.Failure("cannot.add.ancestor", "Cannot add child as ancestor");
        }
    }
}