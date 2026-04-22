using Mediatr.FluentBehavior.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Mediatr.FluentBehavior.Extensions;

public static class Extensions
{
    public static T ConstructByServiceProvider<T>(this IServiceProvider services)
    {
        var type = typeof(T);
        var ctor = type.GetConstructors().FirstOrDefault();
        if (ctor == null)
            throw new CtorNotFoundException(type);
        
        return (T)ctor.Invoke(
            ctor.GetParameters()
                .Select(s => services.GetRequiredService(s.ParameterType))
                .ToArray());
    }
}