namespace Mediatr.FluentBehavior.Interfaces;

/// <summary>
/// Построитель пайплайна обработки
/// </summary>
public interface IPipelineBuilder<out TRequest, TResponse>
{
    /// <summary>
    /// Добавление декоратора
    /// </summary>
    IPipelineBuilder<TRequest, TResponse> WithBehavior<TBehavior>()
        where TBehavior : IFluentBehavior<TRequest, TResponse>;
    
    /// <summary>
    /// Добавление декоратора
    /// </summary>
    IPipelineBuilder<TRequest, TResponse> WithBehavior(IFluentBehavior<TRequest, TResponse> behavior);

    /// <summary>
    /// Запустить пайплайн
    /// </summary>
    Task<TResponse> ExecuteAsync(CancellationToken cancellationToken = default);
}