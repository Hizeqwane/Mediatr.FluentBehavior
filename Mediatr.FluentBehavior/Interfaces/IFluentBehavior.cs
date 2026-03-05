namespace Mediatr.FluentBehavior.Interfaces;

/// <summary>
/// Декоратор
/// </summary>
public interface IFluentBehavior<in TRequest, TResponse>
{
    /// <summary>
    /// Обработка
    /// </summary>
    Task<TResponse> Handle(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken);
}