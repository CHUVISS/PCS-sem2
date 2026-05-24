using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace TouristGuide.Helpers;

public class ImageHelper
{
    private static readonly HttpClient Http = new();

    static ImageHelper()
    {
        Http.DefaultRequestHeaders.UserAgent.ParseAdd("TouristGuide/1.0 (educational project)");
        Http.Timeout = TimeSpan.FromSeconds(15);
    }

    public static readonly AttachedProperty<string?> SourceProperty =
        AvaloniaProperty.RegisterAttached<ImageHelper, Image, string?>("Source");

    public static void Initialize()
    {
        SourceProperty.Changed.Subscribe(e =>
        {
            if (e.Sender is Image img)
            {
                var url = e.NewValue.GetValueOrDefault();
                if (!string.IsNullOrWhiteSpace(url))
                {
                    Console.WriteLine($"[ImageHelper] Loading: {url}");
                    _ = LoadAsync(img, url);
                }
            }
        });
    }

    private static async Task LoadAsync(Image image, string url)
    {
        try
        {
            var bytes = await Http.GetByteArrayAsync(url).ConfigureAwait(false);
            Console.WriteLine($"[ImageHelper] Downloaded {bytes.Length} bytes from {url}");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                using var ms = new MemoryStream(bytes);
                image.Source = new Bitmap(ms);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ImageHelper] ERROR loading {url}: {ex.Message}");
        }
    }

    public static string? GetSource(Image image) => image.GetValue(SourceProperty);
    public static void SetSource(Image image, string? value) => image.SetValue(SourceProperty, value);
}
