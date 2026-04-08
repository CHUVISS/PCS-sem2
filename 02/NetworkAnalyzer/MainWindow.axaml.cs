using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace NetworkAnalyzer;

public partial class MainWindow : Window
{
    private readonly List<string> _urlHistory = new();

    public MainWindow()
    {
        InitializeComponent();
        LoadNetworkInterfaces();
    }

    //  СЕТЕВЫЕ ИНТЕРФЕЙСЫ
    private void LoadNetworkInterfaces()
    {
        ListInterfaces.Items.Clear();

        var interfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var ni in interfaces)
            ListInterfaces.Items.Add(ni.Name);

        TxtStatus.Text = $"Найдено интерфейсов: {interfaces.Length}";
        TxtStatus.Foreground = Brush.Parse("#A6E3A1");
    }

    private void BtnRefresh_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LoadNetworkInterfaces();
        ClearInterfaceInfo();
    }

    private void ListInterfaces_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ListInterfaces.SelectedItem is not string selectedName) return;

        var ni = NetworkInterface
            .GetAllNetworkInterfaces()
            .FirstOrDefault(x => x.Name == selectedName);

        if (ni == null) return;
        ShowInterfaceInfo(ni);
    }

    private void ShowInterfaceInfo(NetworkInterface ni)
    {
        TxtIfName.Text  = ni.Name;
        TxtIfType.Text  = ni.NetworkInterfaceType.ToString();
        TxtIfStatus.Text = ni.OperationalStatus.ToString();
        TxtIfSpeed.Text  = FormatSpeed(ni.Speed);

        // MAC
        var mac = ni.GetPhysicalAddress().GetAddressBytes();
        TxtIfMac.Text = mac.Length == 0
            ? "—"
            : string.Join(":", mac.Select(b => b.ToString("X2")));

        // IP-свойства
        var props = ni.GetIPProperties();

        var ipv4 = props.UnicastAddresses
            .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
            .ToList();

        TxtIfIp.Text = ipv4.Any()
            ? string.Join(", ", ipv4.Select(a => a.Address.ToString()))
            : "—";

        TxtIfMask.Text = ipv4.Any()
            ? string.Join(", ", ipv4.Select(a => a.IPv4Mask?.ToString() ?? "—"))
            : "—";

        TxtIfGateway.Text = props.GatewayAddresses.Any()
            ? string.Join(", ", props.GatewayAddresses.Select(g => g.Address.ToString()))
            : "—";

        TxtIfDns.Text = props.DnsAddresses.Any()
            ? string.Join(Environment.NewLine, props.DnsAddresses.Select(d => d.ToString()))
            : "—";

        // Статус-строка
        TxtStatus.Text = $"{ni.Name} — {ni.OperationalStatus}";
        TxtStatus.Foreground = ni.OperationalStatus == OperationalStatus.Up
            ? Brush.Parse("#A6E3A1")
            : Brush.Parse("#F38BA8");
    }

    private void ClearInterfaceInfo()
    {
        foreach (var tb in new[] { TxtIfName, TxtIfType, TxtIfMac, TxtIfIp,
                                   TxtIfMask, TxtIfGateway, TxtIfStatus, TxtIfSpeed, TxtIfDns })
            tb.Text = "—";
    }

    private static string FormatSpeed(long bps)
    {
        if (bps <= 0)             return "Нет данных";
        if (bps >= 1_000_000_000) return $"{bps / 1_000_000_000.0:F1} Гбит/с";
        if (bps >= 1_000_000)     return $"{bps / 1_000_000.0:F1} Мбит/с";
        if (bps >= 1_000)         return $"{bps / 1_000.0:F1} Кбит/с";
        return $"{bps} бит/с";
    }

    //  АНАЛИЗ URL / URI
    private void TxtUrl_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) AnalyzeUrl();
    }

    private void BtnAnalyze_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => AnalyzeUrl();

    private void AnalyzeUrl()
    {
        var raw = TxtUrl.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(raw))
        {
            TxtResults.Text = "⚠ Введите URL.";
            return;
        }

        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri))
        {
            TxtResults.Text = "✗ Неверный формат URL. Пример: https://example.com/path?q=1#frag";
            ClearUriFields();
            return;
        }

        TxtUriScheme.Text   = uri.Scheme;
        TxtUriHost.Text     = uri.Host;
        TxtUriPort.Text     = uri.IsDefaultPort ? $"{uri.Port} (по умолчанию)" : uri.Port.ToString();
        TxtUriPath.Text     = string.IsNullOrEmpty(uri.AbsolutePath) ? "—" : uri.AbsolutePath;
        TxtUriQuery.Text    = string.IsNullOrEmpty(uri.Query) ? "—" : ParseQueryString(uri.Query);
        TxtUriFragment.Text = string.IsNullOrEmpty(uri.Fragment) ? "—" : uri.Fragment;
        TxtUriAddrType.Text = GetAddressType(uri.Host);

        TxtResults.Text =
            $"✔ URL разобран успешно\n" +
            $"AbsoluteUri : {uri.AbsoluteUri}\n" +
            $"IsAbsolute  : {uri.IsAbsoluteUri}\n" +
            $"UserInfo    : {(string.IsNullOrEmpty(uri.UserInfo) ? "—" : uri.UserInfo)}";

        AddToHistory(raw);
    }

    private static string ParseQueryString(string query)
    {
        var q = query.TrimStart('?');
        var parts = q.Split('&', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(Environment.NewLine,
            parts.Select(p =>
            {
                var kv = p.Split('=', 2);
                return kv.Length == 2
                    ? $"  {Uri.UnescapeDataString(kv[0])} = {Uri.UnescapeDataString(kv[1])}"
                    : $"  {Uri.UnescapeDataString(p)}";
            }));
    }

    private static string GetAddressType(string host)
    {
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            return "Loopback (localhost)";

        if (!IPAddress.TryParse(host, out var addr))
            return "Доменное имя (DNS)";

        if (IPAddress.IsLoopback(addr)) return "Loopback";

        if (addr.AddressFamily == AddressFamily.InterNetwork)
        {
            var b = addr.GetAddressBytes();
            if (b[0] == 10)                                  return "Локальный (10.0.0.0/8)";
            if (b[0] == 172 && b[1] >= 16 && b[1] <= 31)    return "Локальный (172.16.0.0/12)";
            if (b[0] == 192 && b[1] == 168)                  return "Локальный (192.168.0.0/16)";
            if (b[0] == 169 && b[1] == 254)                  return "Link-local (APIPA)";
        }

        return "Публичный";
    }

    private void ClearUriFields()
    {
        foreach (var tb in new[] { TxtUriScheme, TxtUriHost, TxtUriPort,
                                   TxtUriPath, TxtUriQuery, TxtUriFragment, TxtUriAddrType })
            tb.Text = "—";
    }

    //  PING + DNS
    private async void BtnPing_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var raw = TxtUrl.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(raw))
        {
            TxtResults.Text = "⚠ Введите URL или хост.";
            return;
        }

        string host;
        if (Uri.TryCreate(raw, UriKind.Absolute, out var uri))
            host = uri.Host;
        else
            host = raw;

        BtnPing.IsEnabled = false;
        TxtResults.Text   = $"Пингую {host}…";

        var sb = new StringBuilder();
        sb.AppendLine($"=== PING {host} ===");

        try
        {
            var pingTask = PingHostAsync(host, 4,
                line => Dispatcher.UIThread.Post(() =>
                    TxtResults.Text = sb + line)); // live update

            var dnsTask = ResolveDnsAsync(host);

            var pingResult = await pingTask;
            var dnsResult  = await dnsTask;

            sb.Append(pingResult);
            sb.AppendLine();
            sb.Append(dnsResult);
        }
        catch (Exception ex)
        {
            sb.AppendLine($"Ошибка: {ex.Message}");
        }
        finally
        {
            BtnPing.IsEnabled = true;
        }

        TxtResults.Text = sb.ToString();
        AddToHistory(raw);
    }

    private static async Task<string> PingHostAsync(string host, int count,
        Action<string>? onLine = null)
    {
        var sb   = new StringBuilder();
        using var ping = new Ping();
        var opt  = new PingOptions { Ttl = 64, DontFragment = true };
        var buf  = new byte[32];

        long totalRtt = 0;
        int  success  = 0;

        for (int i = 0; i < count; i++)
        {
            try
            {
                var reply = await ping.SendPingAsync(host, 3000, buf, opt);
                string line;
                if (reply.Status == IPStatus.Success)
                {
                    line = $"  [{i + 1}] Ответ от {reply.Address}: {reply.RoundtripTime} мс  TTL={reply.Options?.Ttl}\n";
                    totalRtt += reply.RoundtripTime;
                    success++;
                }
                else
                {
                    line = $"  [{i + 1}] {reply.Status}\n";
                }
                sb.Append(line);
                onLine?.Invoke(line);
            }
            catch (Exception ex)
            {
                sb.AppendLine($"  [{i + 1}] Ошибка: {ex.Message}");
            }

            await Task.Delay(300);
        }

        sb.AppendLine();
        sb.AppendLine($"  Отправлено: {count}  Получено: {success}  Потеряно: {count - success}");
        if (success > 0)
            sb.AppendLine($"  Среднее: {totalRtt / success} мс");

        return sb.ToString();
    }

    private static async Task<string> ResolveDnsAsync(string host)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== DNS {host} ===");
        try
        {
            var entry = await Dns.GetHostEntryAsync(host);
            sb.AppendLine($"  Имя хоста : {entry.HostName}");
            foreach (var a in entry.AddressList)
                sb.AppendLine($"  Адрес     : {a}  ({a.AddressFamily})");
            if (entry.Aliases.Any())
                sb.AppendLine($"  Псевдонимы: {string.Join(", ", entry.Aliases)}");
        }
        catch (SocketException ex)
        {
            sb.AppendLine($"  DNS-ошибка: {ex.Message}");
        }
        return sb.ToString();
    }

    //  ИСТОРИЯ
    private void AddToHistory(string url)
    {
        if (_urlHistory.Contains(url)) return;
        _urlHistory.Insert(0, url);
        if (_urlHistory.Count > 50) _urlHistory.RemoveAt(_urlHistory.Count - 1);

        ListHistory.Items.Clear();
        foreach (var h in _urlHistory)
            ListHistory.Items.Add(h);
    }

    private void BtnClearHistory_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _urlHistory.Clear();
        ListHistory.Items.Clear();
    }

    private void ListHistory_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ListHistory.SelectedItem is string url)
        {
            TxtUrl.Text = url;
            AnalyzeUrl();
        }
    }
}
