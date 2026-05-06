using System;

namespace HttpMonitor.Models;

public class RequestLogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Method { get; set; } = "";
    public string Url { get; set; } = "";
    public int StatusCode { get; set; }
    public double DurationMs { get; set; }
    public string Body { get; set; } = "";
    public string Direction { get; set; } = "IN"; // IN or OUT
    public string Headers { get; set; } = "";

    public string FormattedTime => Timestamp.ToString("HH:mm:ss.fff");
    public string Display =>
        $"[{FormattedTime}] {Direction} {Method} {Url} → {StatusCode} ({DurationMs:F0}ms)";
}

public class ServerStats
{
    public int TotalRequests { get; set; }
    public int GetRequests { get; set; }
    public int PostRequests { get; set; }
    public double AvgDurationMs { get; set; }
    public TimeSpan Uptime { get; set; }
    public string UptimeStr => $"{(int)Uptime.TotalHours:D2}:{Uptime.Minutes:D2}:{Uptime.Seconds:D2}";
}

public class StoredMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Message { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class MinuteStats
{
    public DateTime Minute { get; set; }
    public int Count { get; set; }
}
