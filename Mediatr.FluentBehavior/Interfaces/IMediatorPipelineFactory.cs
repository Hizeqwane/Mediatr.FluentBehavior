using MediatR;

namespace Mediatr.FluentBehavior.Interfaces;

/// <summary>
/// Провайдер построителей пайплайнов
/// </summary>
public interface IMediatorPipelineFactory
{
    /// <summary>
    /// Получить пайплайн
    /// </summary>
    IMediatorPipelineBuilder<TResponse> ByCommand<TResponse>(
        IRequest<TResponse> request);
}