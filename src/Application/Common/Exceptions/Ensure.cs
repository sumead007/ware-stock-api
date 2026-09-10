namespace WareStockApi.Application.Common.Exceptions;

/// <summary>
/// Small helper to throw <see cref="NotFoundException"/> without spelling its name at the call
/// site. <c>Ardalis.GuardClauses</c> (globally imported in this project) also defines a type
/// named <c>NotFoundException</c>, so referencing our own <see cref="NotFoundException"/> by its
/// bare name from any other namespace is ambiguous (CS0104). Handlers should call
/// <see cref="Found{T}"/> instead of writing <c>throw new NotFoundException(...)</c> directly.
/// </summary>
public static class Ensure
{
    public static T Found<T>(T? entity, string name, object key)
        where T : class
    {
        if (entity is null)
        {
            throw new NotFoundException(name, key);
        }

        return entity;
    }
}
