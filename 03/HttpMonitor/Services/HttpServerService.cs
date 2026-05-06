using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpMonitor.Models;
using Newtonsoft.Json;

namespace HttpMonitor.Services;

public class HttpServerService
{
    private HttpListener? _listener;
    private CancellationTokenSource? _cts;
    private readonly List<StoredMessage> _messages = new();
    private readonly object _lock = new();
    private DateTime _startTime;

    public event Action<RequestLogEntry>? RequestReceived;
    public bool IsRunning { get; private set; }

    public void Start(int port)
    {
        if (IsRunning) return;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{port}/");
        _listener.Start();
        _cts = new CancellationTokenSource();
        _startTime = DateTime.Now;
        IsRunning = true;

        Task.Run(() => ListenLoop(_cts.Token));
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
        _listener = null;
        IsRunning = false;
    }

    private async Task ListenLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _listener != null)
        {
            try
            {
                var ctx = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequest(ctx), ct);
            }
            catch { break; }
        }
    }

    private async Task HandleRequest(HttpListenerContext ctx)
    {
        var sw = Stopwatch.StartNew();
        var req = ctx.Request;
        var res = ctx.Response;
        string body = "";

        try
        {
            if (req.HasEntityBody)
            {
                using var reader = new StreamReader(req.InputStream, req.ContentEncoding);
                body = await reader.ReadToEndAsync();
            }

            int statusCode = 200;
            string responseBody = "";

            if (req.HttpMethod == "GET")
            {
                var stats = GetStats();
                responseBody = JsonConvert.SerializeObject(new
                {
                    status = "running",
                    uptime = stats.UptimeStr,
                    totalRequests = stats.TotalRequests,
                    getRequests = stats.GetRequests,
                    postRequests = stats.PostRequests,
                    avgDurationMs = stats.AvgDurationMs
                }, Formatting.Indented);
            }
            else if (req.HttpMethod == "POST")
            {
                try
                {
                    dynamic? json = JsonConvert.DeserializeObject(body);
                    string msg = json?.message ?? "";
                    var stored = new StoredMessage { Message = msg };
                    lock (_lock) { _messages.Add(stored); }
                    statusCode = 201;
                    responseBody = JsonConvert.SerializeObject(new { id = stored.Id, message = stored.Message });
                }
                catch
                {
                    statusCode = 400;
                    responseBody = JsonConvert.SerializeObject(new { error = "Invalid JSON" });
                }
            }
            else
            {
                statusCode = 405;
                responseBody = JsonConvert.SerializeObject(new { error = "Method not allowed" });
            }

            sw.Stop();
            var bytes = Encoding.UTF8.GetBytes(responseBody);
            res.StatusCode = statusCode;
            res.ContentType = "application/json";
            res.ContentLength64 = bytes.Length;
            await res.OutputStream.WriteAsync(bytes);
            res.Close();

            var entry = new RequestLogEntry
            {
                Method = req.HttpMethod,
                Url = req.Url?.ToString() ?? "",
                StatusCode = statusCode,
                DurationMs = sw.Elapsed.TotalMilliseconds,
                Body = body,
                Direction = "IN",
                Headers = req.Headers.ToString() ?? ""
            };

            LogToFile(entry);
            RequestReceived?.Invoke(entry);
        }
        catch
        {
            try { res.Close(); } catch { }
        }
    }

    public ServerStats GetStats() => new ServerStats
    {
        Uptime = DateTime.Now - _startTime,
        TotalRequests = 0 // populated from ViewModel
    };

    private void LogToFile(RequestLogEntry entry)
    {
        try
        {
            File.AppendAllText("logs.txt",
                $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {entry.Direction} {entry.Method} {entry.Url} " +
                $"Status:{entry.StatusCode} Duration:{entry.DurationMs:F0}ms Body:{entry.Body}\n");
        }
        catch { }
    }
}
