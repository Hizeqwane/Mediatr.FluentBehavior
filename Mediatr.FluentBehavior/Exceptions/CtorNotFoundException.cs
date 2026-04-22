namespace Mediatr.FluentBehavior.Exceptions;

public class CtorNotFoundException(Type type) : Exception(
    $"Для типа {type.Name} не найден конструктор.");