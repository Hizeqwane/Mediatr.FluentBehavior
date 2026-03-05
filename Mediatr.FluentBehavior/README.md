# MediatR.FluentBehavior

Библиотека для динамического добавления поведений (декораторов) к запросам MediatR на этапе вызова.

## Мотивация

MediatR предоставляет механизм пайплайнов через `IPipelineBehavior`. Однако стандартный подход имеет ограничение: поведения регистрируются глобально в DI-контейнере и применяются ко всем запросам определённого типа.

Данная библиотека решает эту проблему, позволяя добавлять поведения непосредственно в момент вызова команды.

## Возможности

- Добавление поведений к конкретному вызову команды
- Fluent-синтаксис для удобной конфигурации
- Поддержка DI для поведений, требующих зависимостей
- Сохранение типобезопасности на всех этапах

## Установка

На данный момент библиотека не опубликована в NuGet. Для использования скопируйте исходный код в свой проект или добавьте как ссылку на проект.

## Использование

### 1. Регистрация в DI-контейнере

```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
services.AddTransient(typeof(IMediatrPipelineBuilder<>), typeof(MediatorPipelineBuilder<>));
```

### 2. Создание поведения

Реализуйте интерфейс IFluentBehavior<TRequest, TResponse>:

```csharp
public interface IFluentBehavior<in TRequest, TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken);
}
```

Примеры реализаций различных поведений (логирование, повторные попытки) доступны в репозитории (проект Demo).

### 3. Регистрация extension-методов

Для удобства использования рекомендуется создавать extension-методы для IMediatrPipelineBuilder<TResponse>:

```csharp
public static class MediatorPipelineBuilderExtensions
{
    extension<TResponse>(IMediatrPipelineBuilder<TResponse> builder)
    {
        public IMediatrPipelineBuilder<TResponse> WithCustomBehavior()
        {
            return builder.WithBehavior(sp =>
                {
                    var dependency = sp.GetRequiredService<ICustomDependency>();
                    return new CustomBehavior<IRequest<TResponse>, TResponse>(dependency);
                });
        }
    }
}
```

### 4. Использование в приложении

```csharp
public class MyService
{
    private readonly IMediator _mediator;

    public MyService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<string> ProcessAsync()
    {
        var command = new MyCommand();

        return await _mediator.CreatePipeline(command)
            .WithCustomBehavior()       // добавление поведения
            .WithAnotherBehavior()      // ещё одно поведение
            .ExecuteAsync();            // выполнение
    }
}
```

## Архитектура выполнения

```text
IMediator.CreatePipeline() → MediatorPipelineBuilder<T>
                                        ↓
                                .WithBehavior1()
                                .WithBehavior2()
                                        ↓
                          Накопление поведений в списке
                                        ↓
                                .ExecuteAsync()
                                        ↓
            Построение цепочки: Behavior2 → Behavior1 → Mediator.Send()
                                        ↓
                        Выполнение через IMediator.Send()
```

Поведения выполняются в порядке, обратном их добавлению. Если поведения добавлены последовательно:

```csharp
.WithBehavior(A)
.WithBehavior(B)
```

то цепочка выполнения будет: B → A → обработчик.