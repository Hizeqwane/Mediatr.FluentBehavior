using Mediatr.FluentBehavior.Interfaces;

namespace Mediatr.FluentBehavior.Exceptions;

public class CommandNotSetException() : Exception(
    $"Для обработки пайплайна не задана команда. Используйте {nameof(IMediatrPipelineBuilder<>.SetCommand)} для задания команды");