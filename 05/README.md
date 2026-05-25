# 🏭 Интерактивная система управления производством
### ASP.NET Core MVC + Entity Framework Core + SQLite

---

## 🖥️ Установка и запуск на macOS (M2 Pro / Apple Silicon)

### 1. Установить .NET 8 SDK (ARM64)

```bash
# Через Homebrew (рекомендуется)
brew install --cask dotnet-sdk

# Или скачать напрямую с официального сайта:
# https://dotnet.microsoft.com/download/dotnet/8.0
# → выбрать: macOS | Arm64 | SDK
```

Проверить установку:
```bash
dotnet --version   # должно быть 8.x.x
dotnet --info      # убедиться что RID = osx-arm64
```

---

### 2. Клонировать / распаковать проект

```bash
cd ~/Documents
# если архив:
unzip ProductionManagement.zip
cd ProductionManagement
```

---

### 3. Восстановить пакеты

```bash
dotnet restore
```

---

### 4. Запустить приложение

```bash
dotnet run
```

После запуска в консоли появится:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

Открыть в браузере: **http://localhost:5000**

База данных `production.db` создаётся **автоматически** при первом запуске.

---

### 5. Горячая перезагрузка (для разработки)

```bash
dotnet watch run
```

Изменения в коде применяются без перезапуска сервера.

---

## 🗄️ Работа с базой данных вручную (опционально)

```bash
# Установить EF Core Tools (один раз)
dotnet tool install --global dotnet-ef

# Создать новую миграцию (если изменили модели)
dotnet ef migrations add НазваниеМиграции

# Применить миграции
dotnet ef database update

# Сбросить БД (удалить и пересоздать)
rm production.db
dotnet run
```

---

## 📁 Структура проекта

```
ProductionManagement/
├── Controllers/
│   ├── MvcControllers.cs       # HomeController, MaterialsController,
│   │                           # ProductsController, OrdersController, LinesController
│   └── ApiControllers.cs       # REST API для всех сущностей
├── Models/
│   └── Models.cs               # Product, ProductionLine, Material,
│                               # ProductMaterial, WorkOrder
├── Data/
│   └── AppDbContext.cs         # EF Core контекст + seed данные
├── Views/
│   ├── Shared/_Layout.cshtml   # Общий макет с навигацией
│   ├── Home/Index.cshtml       # Дашборд
│   ├── Materials/              # CRUD + пополнение запасов
│   ├── Products/               # CRUD + привязка материалов
│   ├── Orders/                 # CRUD + управление заказами
│   └── Lines/                  # Панель производственных линий
├── Migrations/                 # EF Core миграции
├── wwwroot/
│   ├── css/site.css
│   └── js/site.js
├── Program.cs                  # Точка входа + DI
├── appsettings.json            # Строка подключения
└── ProductionManagement.csproj
```

---

## 🌐 REST API (Swagger-совместимые эндпоинты)

| Метод | URL | Описание |
|-------|-----|----------|
| GET | `/api/materials?low_stock=true` | Материалы с низким запасом |
| POST | `/api/materials` | Добавить материал |
| PUT | `/api/materials/{id}/stock` | Изменить количество |
| GET | `/api/products?category=Корпуса` | Список продуктов |
| GET | `/api/products/{id}/materials` | Материалы продукта |
| POST | `/api/products` | Создать продукт |
| GET | `/api/lines?available=true` | Доступные линии |
| PUT | `/api/lines/{id}/status` | Изменить статус |
| GET | `/api/lines/{id}/schedule` | Расписание линии |
| GET | `/api/orders?status=active` | Фильтрация заказов |
| POST | `/api/orders` | Создать заказ |
| PUT | `/api/orders/{id}/progress` | Обновить прогресс |
| GET | `/api/orders/{id}/details` | Детали заказа |
| POST | `/api/calculate/production` | Рассчитать время |

### Примеры curl-запросов:

```bash
# Получить материалы с низким запасом
curl http://localhost:5000/api/materials?low_stock=true

# Создать материал
curl -X POST http://localhost:5000/api/materials \
  -H "Content-Type: application/json" \
  -d '{"name":"Алюминий","quantity":200,"unit":"кг","min_stock":50}'

# Рассчитать время производства
curl -X POST http://localhost:5000/api/calculate/production \
  -H "Content-Type: application/json" \
  -d '{"product_id":1,"quantity":10}'

# Создать заказ
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{"product_id":1,"quantity":5,"line_id":1}'

# Обновить прогресс заказа
curl -X PUT http://localhost:5000/api/orders/1/progress \
  -H "Content-Type: application/json" \
  -d '{"percent":75}'
```

---

## 🧩 Функционал по модулям

### 📦 Материалы
- Таблица с цветовой индикацией низкого запаса (красная строка)
- Быстрое пополнение через модальное окно
- Добавление / редактирование / удаление

### 🔧 Продукты
- Фильтр по категории + поиск по названию
- Привязка необходимых материалов (несколько)
- Просмотр спецификации и нормы расхода

### 📋 Заказы
- Фильтрация по статусу (Pending / InProgress / Completed / Cancelled)
- Автоматический расчёт срока при создании
- Запуск, отмена, обновление прогресса через слайдер

### 🏗️ Производственные линии
- Карточки с визуальным статусом (зелёный/серый)
- Регулировка коэффициента эффективности (0.5× — 2.0×)
- Расписание назначенных заказов
- Переключение статуса одной кнопкой

---

## ⚙️ Формула расчёта времени

```
Время (мин) = (Количество × ВремяНаЕдиницу) / КоэффЭффективности
```

Пример: 10 шт × 120 мин / 1.2 = **1000 минут ≈ 16.7 часов**

---

## 🛠️ Возможные проблемы

| Проблема | Решение |
|----------|---------|
| `dotnet: command not found` | Переоткрыть Terminal после установки, или `export PATH=$PATH:/usr/local/share/dotnet` |
| Порт 5000 занят | `dotnet run --urls http://localhost:5001` |
| Ошибка миграции | `rm production.db && dotnet run` |
| Медленная первая сборка | Нормально — NuGet загружает пакеты (~1-2 мин) |
