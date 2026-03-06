using MediatR;

namespace Mediatr.FluentBehavior.Interfaces;

/// <summary>
/// Построитель пайплайна обработки
/// </summary>
public interface IMediatorPipelineBuilder<TResponse>
{
    /// <summary>
    /// Добавление декоратора
    /// </summary>
    IMediatorPipelineBuilder<TResponse> WithBehavior(IFluentBehavior<IRequest<TResponse>, TResponse> behavior);
    
    /// <summary>
    /// Добавление декоратора с использованием DI
    /// </summary>
    IMediatorPipelineBuilder<TResponse> WithBehavior(
        Func<IServiceProvider, IFluentBehavior<IRequest<TResponse>, TResponse>> behavior);

    /// <summary>
    /// Запустить пайплайн
    /// </summary>
    Task<TResponse> ExecuteAsync(CancellationToken cancellationToken = default);
}