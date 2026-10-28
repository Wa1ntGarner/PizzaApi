# PizzaApi 🍕

Серверное REST API пиццерии на **ASP.NET Core Web API (.NET 10)**.
Этап 1 — создание и первоначальная настройка проекта: два контекста данных,
ASP.NET Core Identity, миграции SQL Server, тестовый проект xUnit и Git-репозиторий.

## 🚀 Стек технологий

| Слой | Технологии |
| --- | --- |
| Платформа | C#, ASP.NET Core Web API (.NET 10), контроллеры |
| База данных | SQL Server (LocalDB) + EF Core 10.0.14 |
| Аутентификация | ASP.NET Core Identity (`IdentityUser` / `IdentityRole`), cookie |
| Тестирование | xUnit 2.9.3 + EF Core InMemory |
| Документация API | XML-комментарии контроллеров и `ProducesResponseType` |

## 📂 Структура решения

```
pizza.slnx                    — решение
pizza/                        — проект API (пространство имён PizzaApi)
  Controllers/                — PizzasController, OrdersController
  DTOs/                       — объекты запросов и ответов
  Models/                     — Pizza, Order, OrderItem, OrderStatus
  Services/                   — PizzaService, OrderService (бизнес-правила)
  Data/
    AppDbContext.cs           — контекст предметной области
    AuthDbContext.cs          — контекст Identity
    Migrations/Auth/          — миграции AuthDbContext
    Migrations/App/           — миграции AppDbContext
PizzaApi.Tests/               — тестовый проект xUnit (12 тестов)
```

## 🍕 Предметная область

**Основная сущность — `Pizza`** (поля помимо `Id`): `Name`, `Description`, `Price`,
`DiameterCm`, `IsVegetarian`, `IsAvailable`, `CreatedAt`.

**Связанная сущность — `Order`** (заказ): `CustomerName`, `CustomerPhone`, `Address`,
`Status`, `TotalPrice`, `CreatedAt` и коллекция позиций `OrderItem`
(`PizzaId`, `Quantity`, `UnitPrice` — цена фиксируется на момент оформления).

Бизнес-правила, вынесенные в сервисы:

* цена пиццы больше нуля, диаметр 20–45 см, название уникально;
* заказ без позиций невозможен;
* количество одной пиццы в позиции — от 1 до 20;
* нельзя заказать недоступную пиццу;
* доставка бесплатна при сумме заказа от 1000 ₽, иначе 149 ₽;
* цена пиццы копируется в позицию заказа и не меняется при изменении меню.


## ⚙️ Настройка и запуск

1. Убедитесь, что установлены .NET SDK 10 и SQL Server (подойдёт LocalDB).
2. Строка подключения находится в `pizza/appsettings.json`:
3. Примените миграции:
4. Запуск API (профиль `http`, HTTPS отключён):

```powershell
dotnet run --project pizza --launch-profile http
```

Приложение слушает `http://localhost:5297`. Проверка: `GET /api/pizzas`
возвращает меню, `GET /api/orders` — 401, `GET /` — 404 (главная страница не нужна).

5. Тесты:

```powershell
dotnet test
```

12 тестов проверяют валидацию меню, фильтрацию недоступных пицц, расчёт стоимости
заказа, бесплатную доставку от 1000 ₽, фиксацию цены и запрет заказа скрытой пиццы.

## ✅ Чек-лист этапа 1

* [x] Web API на контроллерах, HTTPS выключен, профиль `http`.
* [x] Пакеты `Microsoft.EntityFrameworkCore.SqlServer`, `.Tools`,
      `Microsoft.AspNetCore.Identity.EntityFrameworkCore` версии 10.0.14.
* [x] `AppDbContext` и `AuthDbContext` созданы и зарегистрированы в DI.
* [x] Оба контекста используют одно подключение (LocalDB, локальный экземпляр).
* [x] Настроены `AddIdentity`, аутентификация и авторизация, cookie возвращает 401/403.
* [x] Миграция `AuthDbContext` применена, таблицы Identity созданы.
* [x] Добавлен xUnit-проект `PizzaApi.Tests` со ссылкой на API.
* [x] Решение собирается, приложение запускается по HTTP, тесты проходят.
* [x] Создан Git-репозиторий с `.gitignore` и первым коммитом.

## 📝 Примечания по локальному окружению

* В репозитории есть `NuGet.config`, который направляет восстановление пакетов на
  публичное зеркало `dotnet-public` (используется, если `nuget.org` недоступен).
  На машине с обычным доступом к `nuget.org` файл можно удалить.
* База данных создаётся миграциями в LocalDB, имя базы — `PizzaApiDb`.
