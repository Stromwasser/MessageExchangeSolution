using MessageExchangeAPI.Hubs;
using MessageExchangeAPI.Repositories;
using Serilog;
using Serilog.Events;
using System.Reflection;

namespace MessageExchangeAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Инициализация Serilog
            // Program.cs (Web API)
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationContext", "MessageExchangeAPI") // Добавляем контекст приложения
                                                                                 // .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName) // Добавляем окружение
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.File("Logs/api-log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();


            try
            {
                Log.Information("Starting up the application...");

                var builder = WebApplication.CreateBuilder(args);

                // Подключаем Serilog
                builder.Host.UseSerilog();
                var port = Environment.GetEnvironmentVariable("API_PORT") ?? "7043";
                builder.WebHost.UseUrls($"http://+:{port}");
                // Получаем текущее окружение (по умолчанию "Development")
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

                // Загружаем конфигурации
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true) // Загружаем доп. конфигурацию
                    .AddEnvironmentVariables()
                    .Build();

                // Получаем строку подключения
                string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Database connection string is not set.");


                builder.Services.AddScoped<IMessageRepository>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<MessageRepository>>();
    return new MessageRepository(connectionString, logger);
});


                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                builder.Services.AddSwaggerGen(c =>
                {
                    c.IncludeXmlComments(xmlPath);
                });

                builder.Services.AddSignalR();
                var clientUrl = Environment.GetEnvironmentVariable("CLIENT_URL") ?? "http://localhost:7082";

                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(builder =>
                    {
                        builder.WithOrigins(clientUrl,
    "https://localhost:5220",
    "http://localhost:5220",
    "http://localhost:7082",
    "http://messageexchange_client",
    "http://messageexchange_api:7043")  // ✅ Добавили API в контейнере
   .AllowAnyMethod()
   .AllowAnyHeader()
   .AllowCredentials()
   .SetIsOriginAllowed(origin => true);

                    });
                });

                builder.Services.AddHttpClient("MessageExchangeClient", client =>
                {
                    client.BaseAddress = new Uri("http://localhost:7043/");
                });

                // Читаем порт из переменной окружения или используем 7043 по умолчанию
               

                var app = builder.Build();

                if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }
                app.UseStaticFiles();
                app.UseCors();
                //    app.UseHttpsRedirection();
                app.UseAuthorization();

                app.MapControllers();
                app.MapHub<MessageHub>("/hubs/message");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush(); // Закрываем логер корректно
            }
        }
    }
}
