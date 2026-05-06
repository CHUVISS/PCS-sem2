using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HttpMonitor.Models;
using Newtonsoft.Json;

namespace HttpMonitor.Services;

public class HttpClientService
{
    private static readonly HttpClient _client = new() { Timeout = TimeSpan.FromSeconds(30) };

    public event Action<RequestLogEntry>? RequestSent;

    public async Task<(string response, int statusCode, double durationMs)> SendAsync(
        string url, string method, string body = "")
    {
        var sw = Stopwatch.StartNew();
        int statusCode = 0;
        string responseText = "";

        try
        {
            HttpResponseMessage response;
            if (method == "GET")
            {
                response = await _client.GetAsync(url);
            }
            else
            {
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                response = await _client.PostAsync(url, content);
            }

            sw.Stop();
            statusCode = (int)response.StatusCode;
            responseText = await response.Content.ReadAsStringAsync();

            // Pretty-print JSON if possible
            try
            {
                var obj = JsonConvert.DeserializeObject(responseText);
                responseText = JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            catch { }

            var entry = new RequestLogEntry
            {
                Method = method,
                Url = url,
                StatusCode = statusCode,
                DurationMs = sw.Elapsed.TotalMilliseconds,
                Body = body,
                Direction = "OUT"
            };

            LogToFile(entry);
            RequestSent?.Invoke(entry);
        }
        catch (Exception ex)
        {
            sw.Stop();
            responseText = $"Error: {ex.Message}";
        }

        return (responseText, statusCode, sw.Elapsed.TotalMilliseconds);
    }

    private void LogToFile(RequestLogEntry entry)
    {
        try
        {
            System.IO.File.AppendAllText("logs.txt",
                $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {entry.Direction} {entry.Method} {entry.Url} " +
                $"Status:{entry.StatusCode} Duration:{entry.DurationMs:F0}ms\n");
        }
        catch { }
    }
}
