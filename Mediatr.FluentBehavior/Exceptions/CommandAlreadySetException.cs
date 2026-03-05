using Mediatr.FluentBehavior.Interfaces;

namespace Mediatr.FluentBehavior.Exceptions;

public class CommandAlreadySetException() : Exception(
    $"Для обработки пайплайна команда уже была задана. Используйте {nameof(IMediatrPipelineBuilder<>.SetCommand)} один раз");