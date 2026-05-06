using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using HttpMonitor.Models;
using HttpMonitor.Services;
using ReactiveUI;

namespace HttpMonitor.ViewModels;

public class MainViewModel : ReactiveObject
{
    private readonly HttpServerService _server = new();
    private readonly HttpClientService _client = new();
    private readonly DispatcherTimer _uptimeTimer;
    private DateTime _serverStartTime;
    private readonly List<RequestLogEntry> _allLogs = new();

    // ── Server ────────────────────────────────────────────────────
    private string _serverPort = "8080";
    public string ServerPort { get => _serverPort; set => this.RaiseAndSetIfChanged(ref _serverPort, value); }

    private bool _serverRunning;
    public bool ServerRunning { get => _serverRunning; set => this.RaiseAndSetIfChanged(ref _serverRunning, value); }

    private string _serverStatusText = "Stopped";
    public string ServerStatusText { get => _serverStatusText; set => this.RaiseAndSetIfChanged(ref _serverStatusText, value); }

    private string _uptime = "00:00:00";
    public string Uptime { get => _uptime; set => this.RaiseAndSetIfChanged(ref _uptime, value); }

    // ── Stats ─────────────────────────────────────────────────────
    private int _totalRequests;
    public int TotalRequests { get => _totalRequests; set => this.RaiseAndSetIfChanged(ref _totalRequests, value); }

    private int _getRequests;
    public int GetRequests { get => _getRequests; set => this.RaiseAndSetIfChanged(ref _getRequests, value); }

    private int _postRequests;
    public int PostRequests { get => _postRequests; set => this.RaiseAndSetIfChanged(ref _postRequests, value); }

    private double _avgDuration;
    public double AvgDuration { get => _avgDuration; set => this.RaiseAndSetIfChanged(ref _avgDuration, value); }

    // ── Client ────────────────────────────────────────────────────
    private string _clientUrl = "https://jsonplaceholder.typicode.com/posts/1";
    public string ClientUrl { get => _clientUrl; set => this.RaiseAndSetIfChanged(ref _clientUrl, value); }

    private string _clientMethod = "GET";
    public string ClientMethod { get => _clientMethod; set => this.RaiseAndSetIfChanged(ref _clientMethod, value); }

    private string _requestBody = "{\n  \"message\": \"Hello from HttpMonitor!\"\n}";
    public string RequestBody { get => _requestBody; set => this.RaiseAndSetIfChanged(ref _requestBody, value); }

    private string _responseText = "";
    public string ResponseText { get => _responseText; set => this.RaiseAndSetIfChanged(ref _responseText, value); }

    private bool _isSending;
    public bool IsSending { get => _isSending; set => this.RaiseAndSetIfChanged(ref _isSending, value); }

    private int _responseStatus;
    public int ResponseStatus { get => _responseStatus; set => this.RaiseAndSetIfChanged(ref _responseStatus, value); }

    private double _responseDuration;
    public double ResponseDuration { get => _responseDuration; set => this.RaiseAndSetIfChanged(ref _responseDuration, value); }

    // ── Logs ──────────────────────────────────────────────────────
    public ObservableCollection<RequestLogEntry> FilteredLogs { get; } = new();

    private string _logFilter = "ALL";
    public string LogFilter
    {
        get => _logFilter;
        set { this.RaiseAndSetIfChanged(ref _logFilter, value); ApplyFilter(); }
    }

    // ── Chart ─────────────────────────────────────────────────────
    public ObservableCollection<MinuteStats> ChartData { get; } = new();

    // ── Commands ──────────────────────────────────────────────────
    public ReactiveCommand<Unit, Unit> ToggleServerCommand { get; }
    public ReactiveCommand<Unit, Unit> SendRequestCommand { get; }
    public ReactiveCommand<string, Unit> SetMethodCommand { get; }
    public ReactiveCommand<string, Unit> SetFilterCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearLogsCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveLogsCommand { get; }
    public ReactiveCommand<string, Unit> SetUrlPresetCommand { get; }

    public MainViewModel()
    {
        // All commands execute on the UI thread scheduler — this is the key fix
        var uiScheduler = RxApp.MainThreadScheduler;

        ToggleServerCommand    = ReactiveCommand.Create(ToggleServer, outputScheduler: uiScheduler);
        SendRequestCommand     = ReactiveCommand.CreateFromTask(SendRequest, outputScheduler: uiScheduler);
        SetMethodCommand       = ReactiveCommand.Create<string>(m => ClientMethod = m, outputScheduler: uiScheduler);
        SetFilterCommand       = ReactiveCommand.Create<string>(f => LogFilter = f, outputScheduler: uiScheduler);
        ClearLogsCommand       = ReactiveCommand.Create(ClearLogs, outputScheduler: uiScheduler);
        SaveLogsCommand        = ReactiveCommand.Create(SaveLogs, outputScheduler: uiScheduler);
        SetUrlPresetCommand    = ReactiveCommand.Create<string>(u => ClientUrl = u, outputScheduler: uiScheduler);

        _server.RequestReceived += OnRequestReceived;
        _client.RequestSent     += OnRequestReceived;

        _uptimeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _uptimeTimer.Tick += (_, _) =>
        {
            if (ServerRunning)
            {
                var up = DateTime.Now - _serverStartTime;
                Uptime = $"{(int)up.TotalHours:D2}:{up.Minutes:D2}:{up.Seconds:D2}";
            }
        };
        _uptimeTimer.Start();

        UpdateChart();
    }

    // ── Server toggle ─────────────────────────────────────────────
    private void ToggleServer()
    {
        if (!ServerRunning)
        {
            if (!int.TryParse(ServerPort, out int port) || port < 1 || port > 65535)
                return;
            try
            {
                _server.Start(port);
                _serverStartTime = DateTime.Now;
                ServerRunning = true;
                ServerStatusText = $"Running on :{port}";
            }
            catch (Exception ex)
            {
                ServerStatusText = $"Error: {ex.Message}";
            }
        }
        else
        {
            _server.Stop();
            ServerRunning = false;
            ServerStatusText = "Stopped";
            Uptime = "00:00:00";
        }
    }

    // ── Send HTTP request ─────────────────────────────────────────
    private async Task SendRequest()
    {
        if (string.IsNullOrWhiteSpace(ClientUrl)) return;

        // IsSending is changed on UI thread because SendRequestCommand uses uiScheduler
        IsSending = true;
        ResponseText = "";
        try
        {
            var (resp, status, dur) = await _client.SendAsync(ClientUrl, ClientMethod, RequestBody);
            // Back on UI thread thanks to outputScheduler
            ResponseText = resp;
            ResponseStatus = status;
            ResponseDuration = dur;
        }
        finally
        {
            IsSending = false;
        }
    }

    // ── Incoming log entry (called from background thread) ────────
    private void OnRequestReceived(RequestLogEntry entry)
    {
        // Always marshal to UI thread before touching ObservableCollection or ReactiveObject props
        Dispatcher.UIThread.Post(() =>
        {
            _allLogs.Insert(0, entry);
            if (_allLogs.Count > 500) _allLogs.RemoveAt(_allLogs.Count - 1);
            ApplyFilter();
            UpdateStats();
            UpdateChart();
        });
    }

    private void ApplyFilter()
    {
        FilteredLogs.Clear();
        var source = LogFilter switch
        {
            "GET"  => _allLogs.Where(l => l.Method == "GET"),
            "POST" => _allLogs.Where(l => l.Method == "POST"),
            "IN"   => _allLogs.Where(l => l.Direction == "IN"),
            "OUT"  => _allLogs.Where(l => l.Direction == "OUT"),
            "2xx"  => _allLogs.Where(l => l.StatusCode >= 200 && l.StatusCode < 300),
            "4xx+" => _allLogs.Where(l => l.StatusCode >= 400),
            _      => _allLogs.AsEnumerable()
        };
        foreach (var e in source.Take(100))
            FilteredLogs.Add(e);
    }

    private void UpdateStats()
    {
        TotalRequests = _allLogs.Count(l => l.Direction != "SYS");
        GetRequests   = _allLogs.Count(l => l.Method == "GET");
        PostRequests  = _allLogs.Count(l => l.Method == "POST");
        var durs = _allLogs.Where(l => l.DurationMs > 0).Select(l => l.DurationMs).ToList();
        AvgDuration   = durs.Count > 0 ? Math.Round(durs.Average(), 1) : 0;
    }

    private void UpdateChart()
    {
        var now = DateTime.Now;
        ChartData.Clear();
        for (int i = -9; i <= 0; i++)
        {
            var bucket = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0)
                             .AddMinutes(i);
            var count = _allLogs.Count(l => l.Timestamp >= bucket && l.Timestamp < bucket.AddMinutes(1));
            ChartData.Add(new MinuteStats { Minute = bucket, Count = count });
        }
    }

    private void ClearLogs()
    {
        _allLogs.Clear();
        FilteredLogs.Clear();
        TotalRequests = GetRequests = PostRequests = 0;
        AvgDuration = 0;
        UpdateChart();
    }

    private void SaveLogs()
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# HttpMonitor Export — {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"# Entries: {_allLogs.Count}");
            sb.AppendLine();
            foreach (var e in _allLogs)
                sb.AppendLine(e.Display);
            File.WriteAllText("logs_export.txt", sb.ToString());
        }
        catch { }
    }
}
