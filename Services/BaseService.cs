using RotaDoSabor.Server.Validations;

namespace RotaDoSabor.Server.Services;

public abstract class BaseService
{
    private readonly List<ValidationError> _errors = [];
    protected IReadOnlyList<ValidationError> Errors => _errors;

    protected void AddError(string field, string message) => _errors.Add(new ValidationError(field, message));
    protected void ThrowIfInvalid()
    {
        if (_errors.Count > 0) throw new ValidationException(_errors.ToList());
    }
}
