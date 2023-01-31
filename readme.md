# Набор инструментов для работы с EasyNetQ

Востребованы 2 типа доставки сообщений:
- конкретному предопределенному адресату (Send/Receive pattern)
- заранее неизвестному множеству адресатов по подписке (Publish/Subscribe pattern)

Эти возможности предоставляет EasyNetQ, но...

## Какие проблемы возникают при использовании EasyNetQ?

1. "Из коробки" EasyNetQ именует queue и exchange с использованием наименования типа сообщения, его пространства имен, сборки.
2. Обработка сообщений происходит через делегаты, что мешает использованию DI и паттерна UnitOfWork. В случае с Publish/Subscribe есть решение через [AutoSubscriber](https://github.com/EasyNetQ/EasyNetQ/wiki/Auto-Subscriber), но подобное отсутсвует для Send/Receive.

## Заложенные соглашения

Подключая данный набор, вы принимаете следующие соглашения, которые влияют на работу шины:
1. Тип сообщений контролирует `MapTypeNameSerializer`, который через конфигурацию строго ограничивает набор возможных сообщений для публикации/потребления. Как следствие, названия создаваемых queue и exchange для сообщений типа Publish/Subscribe становятся управляемыми: либо это наименование типа сообщения (без пространства имен, сборки), либо некая константа.
2. Переопределяется наименование создаваемых queue и exchange при необходимости поместить ошибочное сообщение (см. `CustomConventions`)

## Быстрое подключение набора инструментов и используемых типов сообщений

```
services.RegisterMq(
    Configuration,
    // ниже достаточно указать известные типы, которые собираемся отправлять,
    // т. к. типы сообщений для консьюмеров будут указаны при их регистрации (см. Связывание потребителей сообщений с очередью)
    serializer => serializer
        .Use<DoSomethingMqDto>()
        .Use<SomethingHappenedMqDto>()
        .Use("SpecificMessage", DoAnotherThingMqDto));
```
Предполагается, что в `appsettings.json` уже есть параметры подключения к `RabbitMQ`.
```
"RabbitMqConfig": {
    "username": "guest",
    "password": "guest",
    "host": "localhost",
    "publisherConfirms": true
},
```
Полный список параметров описан в `RabbitMqConfig`.

## Реализация потребителей
Происходит через реализацию интерфейса `IMessageConsumer<TMessage>`
```
public class DoSomethingConsumer : Guuyb.Mq.IMessageConsumer<DoSomethingMqDto>
{
    ...
}
```

## Связывание потребителей сообщений с очередью

Предполагается, что в `appsettings.json` есть конфигурация `ConsumerBindingServiceConfig`.
```
"ConsumerBindingServiceConfig": {
    "QueueName": "SomeMicroService_IncomingQueue", // обязательный; будет либо создана при IsNeedToDeclare = true, либо проверка существования на старте
    "PrefetchCount": 10, // опциональный; по умолчанию 1
    "IsNeedToDeclare": false, // опциональный; по умолчанию true
    "Bindings": { // опциональный; будет либо создан Exchange при IsNeedToDeclare = true, либо проверка существования на старте
        "Exchenge0": [ "*" ],
        "Exchenge1": [ "RoutingKey1", "RoutingKey2" ] // Binding'и будут созданы при IsNeedToDeclare = true
    }
},
```
Связывание потребителей:
```
services.BindConsumers<ConsumerBindingServiceConfig>(
    configuration, r =>
    {
        r.Add<SomethingHappenedMqDto, MicroServiceIncomingQueueConsumer>("SpecificTypeName if need");
        r.Add<SomethingElseHappenedMqDto, MicroServiceIncomingQueueConsumer>("SpecificTypeName if need");
    });
```
