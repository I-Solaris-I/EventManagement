# EventManagement

Сервис управления мероприятиями. ЯНДЕКС_ПРАКТИКУМ 07_charp
## Технологии
- C#
- ASP.NET Core
- FluentValidation
- Bogus
## Целевая платформа проекта
- net 9.0

## Структура проекта
- EventManagement - содержит ASP NET WebApi приложение
- EventManagement.Models - содержит модели приложения
- EventManagement.Services - содержит сервисы приложения
- EventManagement.Context - содержит классы, связанные с хранением данных

## Сборка

Для сборки использовать команду: 
```bash
dotnet build
```

## Запуск
```bash
dotnet run --project EventManagement/EventManagement.csproj -lp http
```

В соответствии с профилем запуска http приложение будет доступно по  адресу: [http://localhost:5091]

## Документация

Описание методов доступно по ссылке [http://localhost:5091/swagger] после запуска приложения 