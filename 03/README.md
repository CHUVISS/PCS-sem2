# HTTP Monitor — Avalonia UI

Аналог WPF-приложения для macOS/Linux/Windows на базе **Avalonia UI**.

## Требования

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- macOS / Linux / Windows

## Быстрый старт

```bash
cd HttpMonitor
dotnet restore
dotnet run
```

## Структура проекта

```
HttpMonitor/
├── Models/
│   └── LogEntry.cs           # Модели данных
├── Services/
│   ├── HttpServerService.cs  # HttpListener-сервер
│   ├── HttpClientService.cs  # HttpClient-клиент
│   └── LogService.cs         # Логирование в файл
├── ViewModels/
│   └── MainViewModel.cs      # ReactiveUI ViewModel
├── MainWindow.axaml          # UI разметка (аналог WPF XAML)
├── MainWindow.axaml.cs       # Code-behind
├── App.axaml                 # Точка входа Avalonia
└── Program.cs                # Main()
```

## Функциональность

### Вкладка «Сервер»
- Запуск/остановка HTTP-сервера на заданном порту
- **GET** `/` — возвращает JSON со статусом сервера (uptime, кол-во запросов)
- **POST** `/` — принимает `{"message": "текст"}`, сохраняет, возвращает уникальный ID
- Статистика: GET/POST счётчики, среднее время ответа, uptime
- График нагрузки по минутам
- Фильтрация логов по методу и направлению

### Вкладка «HTTP-Клиент»
- Отправка GET/POST запросов на любой URL
- Красивый вывод JSON-ответа
- Быстрая кнопка «Отправить на локальный сервер»

### Вкладка «Все логи»
- Полный текстовый журнал всех запросов
- Автосохранение в `logs.txt`

## Тест через curl

```bash
# GET — статус сервера
curl http://localhost:8080/

# POST — отправить сообщение
curl -X POST http://localhost:8080/ \
     -H "Content-Type: application/json" \
     -d '{"message": "Привет, сервер!"}'
```
