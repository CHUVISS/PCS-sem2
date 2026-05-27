using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorClient;
using BlazorClient.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient для запросов к серверу
builder.Services.AddScoped(sp => new HttpClient());

// Сервисы приложения
builder.Services.AddScoped<ProductService>();
builder.Services.AddSingleton<ThemeService>();

await builder.Build().RunAsync();
