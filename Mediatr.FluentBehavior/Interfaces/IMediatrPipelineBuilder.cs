using MediatR;

namespace Mediatr.FluentBehavior.Interfaces;

/// <summary>
/// Построитель пайплайна обработки
/// </summary>
public interface IMediatrPipelineBuilder<TResponse>
{
    /// <summary>
    /// Установка базовой команды
    /// </summary>
    IMediatrPipelineBuilder<TResponse> SetCommand(IRequest<TResponse> request);
    
    /// <summary>
    /// Добавление декоратора
    /// </summary>
    IMediatrPipelineBuilder<TResponse> WithBehavior(IFluentBehavior<IRequest<TResponse>, TResponse> behavior);
    
    /// <summary>
    /// Добавление декоратора с использованием DI
    /// </summary>
    IMediatrPipelineBuilder<TResponse> WithBehavior(
        Func<IServiceProvider, IFluentBehavior<IRequest<TResponse>, TResponse>> behavior);

    /// <summary>
    /// Запустить пайплайн
    /// </summary>
    Task<TResponse> ExecuteAsync(CancellationToken cancellationToken = default);
}