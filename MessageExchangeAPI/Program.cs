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
                string connectionString = config.GetConnectionString("DefaultConnection") ??
                    throw new InvalidOperationException("Database connection string is not set.");

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
                var clientUrl = Environment.GetEnvironmentVariable("CLIENT_URL") ?? "https://localhost:7082";

                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(builder =>
                    {
                        builder.WithOrigins(clientUrl,
                            "https://localhost:5220",
                            "http://localhost:5220", 
                            "http://localhost:7082",
                            "http://messageexchangeclient:80")
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
                var app = builder.Build();

                if (app.Environment.IsDevelopment())
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
