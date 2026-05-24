# Туристический путеводитель — Avalonia приложение

Практическое задание по дисциплине «Программирование корпоративных систем».  
Реализовано на **Avalonia UI** + **Entity Framework Core** + **SQLite**.

---

## Требования

- macOS (Apple Silicon M1/M2/M3)
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) — скачать `arm64` версию
- Интернет-соединение (для загрузки NuGet-пакетов и фотографий в приложении)

---

## Быстрый старт

```bash
# 1. Клонируйте / распакуйте проект
cd TouristGuide

# 2. Восстановите пакеты
dotnet restore TouristGuide/TouristGuide.csproj

# 3. Запустите приложение
dotnet run --project TouristGuide/TouristGuide.csproj
```

При первом запуске EF Core автоматически создаёт SQLite-базу данных и заполняет её тестовыми данными (3 города, 6 достопримечательностей).

База хранится по пути:  
`~/Library/Application Support/TouristGuide/tourist_guide.db`

---

## Структура проекта

```
TouristGuide/
├── TouristGuide.sln
└── TouristGuide/
    ├── Models/
    │   ├── City.cs           # Модель города
    │   └── Attraction.cs     # Модель достопримечательности
    ├── Data/
    │   └── AppDbContext.cs   # EF Core контекст + seed данные
    ├── ViewModels/
    │   ├── ViewModelBase.cs
    │   ├── MainViewModel.cs       # Навигация между страницами
    │   ├── CitiesViewModel.cs     # Список городов + поиск
    │   ├── CityDetailViewModel.cs # Детали города
    │   └── AttractionDetailViewModel.cs # Детали достопримечательности
    ├── Views/
    │   ├── MainWindow.axaml      # Главное окно + все шаблоны страниц
    │   └── MainWindow.axaml.cs
    ├── App.axaml
    ├── App.axaml.cs
    └── Program.cs
```

---

## Архитектура

Приложение реализует паттерн **MVVM** (Model-View-ViewModel):

| Слой       | Технология                    | Назначение                           |
|------------|-------------------------------|--------------------------------------|
| Model      | C# классы + EF Core + SQLite  | Хранение данных в БД                 |
| ViewModel  | ReactiveUI + ReactiveCommand  | Логика, навигация, поиск             |
| View       | Avalonia XAML                 | Отображение, DataTemplates-роутинг   |

### Навигация

`MainViewModel.CurrentView` хранит текущую страницу как `ViewModelBase`.  
`ContentControl` в `MainWindow.axaml` автоматически выбирает нужный `DataTemplate` по типу ViewModel.

---

## Функциональность

### Список городов
- Карточки городов с фотографией, названием, регионом, населением
- Поиск по названию и региону (real-time фильтрация)

### Страница города
- Фотография, герб, название, регион, население
- Подробная история города
- Список достопримечательностей с превью

### Страница достопримечательности
- Большая фотография, название, описание
- Часы работы и стоимость посещения
- Подробная история

---

## Технологии

| Пакет                                  | Версия | Назначение                     |
|----------------------------------------|--------|-------------------------------|
| Avalonia                               | 11.1.3 | UI фреймворк                  |
| Avalonia.Themes.Fluent                 | 11.1.3 | Fluent тема оформления        |
| Avalonia.ReactiveUI                    | 11.1.3 | ReactiveUI интеграция          |
| Microsoft.EntityFrameworkCore          | 8.0.0  | ORM                           |
| Microsoft.EntityFrameworkCore.Sqlite   | 8.0.0  | SQLite провайдер              |
| ReactiveUI                             | 20.1.1 | MVVM + реактивные команды     |

---

## Модель данных

```
City
├── Id (PK)
├── Name
├── Region
├── Population
├── History
├── CoatOfArmsUrl
├── PhotoUrl
└── Attractions []

Attraction
├── Id (PK)
├── Name
├── Description
├── History
├── PhotoUrl
├── WorkingHours
├── TicketPrice (nullable)
└── CityId (FK → City)
```
