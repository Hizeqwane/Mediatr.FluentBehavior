using MediatR;

namespace Mediatr.FluentBehavior.Interfaces;

/// <summary>
/// Провайдер построителей пайплайнов для MediatR
/// </summary>
public interface IMediatorPipelineFactory : IPipelineFactory
{
    /// <summary>
    /// Получить пайплайн для MediatR
    /// </summary>
    IPipelineBuilder<IRequest<TResponse>, TResponse> ByMediatorRequest<TResponse>(
        IRequest<TResponse> request) => ByRequest<IRequest<TResponse>, TResponse>(request);
}