# MediatR.FluentBehavior

Библиотека для динамического добавления поведений (декораторов) к запросам MediatR на этапе вызова.

## Мотивация

MediatR предоставляет механизм пайплайнов через `IPipelineBehavior`. Однако, стандартный подход имеет ограничение: поведения регистрируются глобально в DI-контейнере и применяются ко всем запросам определённого типа.

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
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())); // Регистрация Mediatr
services.AddScoped<IMediatorPipelineFactory, MediatorPipelineFactory>();                       // Регистрация фабрики построителей пайплайнов
```

### 2. Создание поведения

Реализуйте интерфейс _IFluentBehavior<TRequest, TResponse>_:

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

Для удобства использования рекомендуется создавать extension-методы для _IMediatrPipelineBuilder\<TResponse>_:

```csharp
public static class MediatorPipelineBuilderExtensions
{
    extension<TResponse>(IMediatrPipelineBuilder<TResponse> builder)
    {
        public IMediatrPipelineBuilder<TResponse> WithCustomBehavior()
        {
            return builder.WithBehavior<CustomBehavior>();
        }
    }
}
```

### 4. Использование в приложении

```csharp
public class MyService(IMediatrPipelineFactory pipelineFactory)
{
    public async Task<string> ProcessAsync()
    {
        var command = new MyCommand();

        return await pipelineFactory
            .ByMediatorRequest(command) // Установка основной команды
            .WithCustomBehavior()       // добавление поведения
            .WithAnotherBehavior()      // ещё одно поведение
            .ExecuteAsync();            // выполнение
    }
}
```

## Архитектура выполнения

```text
                             IMediatorPipelineFactory
                                        ↓
                           .ByMediatorRequest(command)
                                        ↓
                            IMediatorPipelineBuilder<T>
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

## Конфигурирование

На данный момент _BasePipelineBuilder_ резолвит из DI _IOptions\<PipelineBuilderOptions>_ для получения флага _IsBehaviorsInDi_, который влияет на метод _.WithBehaviour\<TBehavior>()_: если флаг = true, то декоратор резолвится из DI, в противном случае все сервисы, которые требует конструктор декоратора будут взяты из DI и декоратор будет создан при добавлении.

## Для реализаций собственных шин обработки команд

Помимо использования _IMediatorPipelineFactory_  для самого _IMediatr_, можно использовать данный механизм и для собственных шин (в том числе и для кастомных обёрток над самим _IMediatr_). 
Для этого рекомендуется сформировать собственный интерфейс (или сразу реализацию) от базового _IPipelineFactory_, которая будет накладывать на TRequest нужный constraint. Конечно, можно использовать и базовый _IPipelineFactory_, но тогда при каждом вызове _.ByRequest<TRequest, TResponse>()_ придётся указывать необходимые типы.   

Сам _MediatorPipelineFactory_ следует того же принципу:
```
public interface IMediatorPipelineFactory : IPipelineFactory
{
    /// <summary>
    /// Получить пайплайн для MediatR
    /// </summary>
    IPipelineBuilder<IRequest<TResponse>, TResponse> ByMediatorRequest<TResponse>(
        IRequest<TResponse> request) => ByRequest<IRequest<TResponse>, TResponse>(request);
}
```

