using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;

namespace MessageExchangeClient;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        // ????????? ????????? ?? appsettings.json
        using var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
        var config = await http.GetFromJsonAsync<AppConfig>("appsettings.json");

        // ????????????? API URL
        var apiBaseUrl = config?.ApiBaseUrl ?? "http://localhost:7043";
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

        await builder.Build().RunAsync();


    }
    public class AppConfig
    {
        public string ApiBaseUrl { get; set; } = string.Empty;
    }
};

