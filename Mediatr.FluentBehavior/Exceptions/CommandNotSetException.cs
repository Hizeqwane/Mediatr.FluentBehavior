namespace Mediatr.FluentBehavior.Exceptions;

public class CommandNotSetException() : Exception(
    "Для обработки пайплайна не задана команда.");