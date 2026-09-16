# EventManagement
Сервис управления мероприятиями. ЯНДЕКС_ПРАКТИКУМ 07_charp
## Технологии
- C#
- ASP.NET Core
- FluentValidation
- Bogus
- xunit.v3
## Целевая платформа проекта
- net 9.0
## Структура проекта
- EventManagement - содержит ASP NET WebApi приложение
- EventManagement.Models - содержит модели приложения
- EventManagement.Services - содержит сервисы приложения
- EventManagement.Context - содержит классы, связанные с хранением данных
- EventManagement.Tests - содержит методы тестирования логики
## Сборка
Для сборки использовать команду: 
```bash
dotnet build
```
## Запуск
Для запуска использовать команду:

```bash
dotnet run --project EventManagement/EventManagement.csproj -lp http
```
В соответствии с профилем запуска http приложение будет доступно по ссылке: [http://localhost:5091](http://localhost:5091)

## Документация
Описание методов доступно по ссылке [http://localhost:5091/swagger](http://localhost:5091/swagger) после запуска приложения.

Запрос **GET/Events** включает следующие параметры для фильтрации:

|Наименование  |Описание                     |Тип               |Опциональный|Значение по умолчанию
|:------------ |:--------                    |:--------         |:--------:  |:--------:
|title         |Фильтр по наименованию       |string            | +          | -
|from          |Фильтрация по дате начала    |string($date-time)| +          | -
|to            |Фильтрация по дате окончания |string($date-time)| +          | -
|page          |Номер страницы               |integer           | +          | 1
|pageSize      |Число элементов на странице  |integer           | +          | 10

Фильтр применяется только если параметры переданы; все фильтры работают одновременно (Логическое И).

**Пример запроса:** [http://localhost:5091/Events?from=2026-09-01&to=2026-09-16&page=1&pageSize=10](http://localhost:5091/Events?from=2026-09-01&to=2026-09-16&page=1&pageSize=10)

**Пример ответа:**
```
{
  "items": [
    {
      "id": "5cd8ef42-7a0d-4f8e-9beb-eec9b498a9ee",
      "title": "Ea nulla dolore iure.",
      "startAt": "2026-09-02T11:25:22.6109258+03:00",
      "endAt": "2026-09-02T12:25:22.6109258+03:00",
      "description": "Aliquam asperiores occaecati optio rem. Perferendis quia et accusamus voluptatem eligendi facilis temporibus quidem. Est magni et sit a quis laboriosam fugit neque. Quaerat nisi maiores praesentium voluptas quisquam occaecati accusantium. Vero numquam ad doloribus aut aspernatur est delectus sunt omnis. Iure nisi assumenda accusantium voluptatum aliquam expedita iusto ipsam."
    }
  ],
  "total": 1,
  "page": 1,
  "count": 1,
  "totalPages": 1,
  "hasNext": false,
  "hasPrevious": false,
  "pageSize": 10
}
```
* items - массив элементов на странице
* total - всего элементов по фильтрации
* page -  номер страницы
* count - число элементов на странице
* totalPages - всего страниц
* hasNext - есть ли следующая страница
* hasPrevious - есть ли предыдущая страница
* pageSize - максимальнов возможное число элементов на странице


**Для ислючительных ситуаций используется формат Problem Details (RFC 7807):** [Ссылка на документ](https://datatracker.ietf.org/doc/html/rfc7807)

## Тестирование

Для запуска тестов использовать команду
```bash
dotnet test
```