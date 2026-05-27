# Практическое задание №6 — Blazor WebAssembly

**Дисциплина:** Программирование корпоративных систем  
**Тема:** Разработка клиент-серверного приложения на Blazor WebAssembly

## Структура проекта

```
06/
├── BlazorServer/          ← ASP.NET Core API (хост)
│   ├── Controllers/
│   │   └── ProductsController.cs   ← REST API: GET/POST/PUT/DELETE
│   ├── Data/
│   │   └── AppDbContext.cs          ← Entity Framework Core (SQLite)
│   ├── Models/
│   │   └── Product.cs
│   └── Program.cs
│
└── BlazorClient/          ← Blazor WebAssembly (клиент)
    ├── Models/
    │   └── Product.cs
    ├── Services/
    │   ├── ProductService.cs        ← HTTP-клиент к API
    │   └── ThemeService.cs          ← Управление темой
    ├── Pages/
    │   ├── Home.razor
    │   └── Products.razor           ← CRUD-страница товаров
    └── Layout/
        ├── MainLayout.razor         ← Переключатель тем
        └── NavMenu.razor
```

## Запуск

Открыть **два** терминала:

### Терминал 1 — сервер (API + БД)
```bash
cd BlazorServer
dotnet run
# Запустится на http://localhost:5000
```

### Терминал 2 — клиент (Blazor WASM)
```bash
cd BlazorClient
dotnet run
# Открыть браузер: http://localhost:5001 (или порт из вывода)
```

## API

| Метод  | Маршрут              | Описание            |
|--------|----------------------|---------------------|
| GET    | /api/products        | Список товаров      |
| POST   | /api/products        | Добавить товар      |
| PUT    | /api/products/{id}   | Изменить товар      |
| DELETE | /api/products/{id}   | Удалить товар       |

## Функциональность

- 📦 Список товаров в таблице
- ➕ Форма добавления нового товара
- ✏️ Форма редактирования товара
- 🗑️ Удаление товара с диалогом подтверждения
- 🌙 / ☀️ Переключатель светлой/тёмной темы
- 💾 Хранение данных в SQLite через Entity Framework Core
