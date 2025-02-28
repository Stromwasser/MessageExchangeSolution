using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace MessageExchangeClient;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        var apiBaseUrl = Environment.GetEnvironmentVariable("API_URL") ?? "https://localhost:7043";
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://messageexchangeapi:80") });



        // builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        await builder.Build().RunAsync();
    }
}
