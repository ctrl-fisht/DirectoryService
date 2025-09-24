using System.Collections;

namespace Shared.Errors;

public class Errors : IEnumerable<Error>
{
    private readonly List<Error> _errors;

    public Errors(Error error)
    {
        _errors =  new List<Error> { error };
    }

    public Errors(IEnumerable<Error> errors)
    {
        _errors = new List<Error>(errors);
    }
    
    public IEnumerator<Error> GetEnumerator()
    {
        return _errors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}