using MessageExchangeDAL.Repositories;
using Serilog;
using Serilog.Events;
using MessageExchangeAPI.Hubs;
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

                // Подключаем зависимости
                string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

                // Регистрация зависимостей
                builder.Services.AddSingleton<IMessageRepository>(provider =>
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
                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(builder =>
                    {
                        builder.WithOrigins("https://localhost:7082")
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials(); // Разрешаем отправку credentials
                    });
                });
                builder.Services.AddHttpClient("MessageExchangeClient", client =>
                {
                    client.BaseAddress = new Uri("https://localhost:7043/");
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
