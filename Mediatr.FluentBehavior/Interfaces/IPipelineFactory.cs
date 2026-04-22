namespace Mediatr.FluentBehavior.Interfaces;

/// <summary>
/// Провайдер построителей пайплайнов
/// </summary>
public interface IPipelineFactory
{
    /// <summary>
    /// Получить пайплайн
    /// </summary>
    IPipelineBuilder<TRequest, TResponse> ByRequest<TRequest, TResponse>(
        TRequest request);
}