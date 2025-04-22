using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace VoiceTexterBot
{
    public class Program
    {
        public static async Task Main()
        {
            Console.OutputEncoding = Encoding.Unicode;

            // Объект, отвечающий за постоянный жизненный цикл приложения
            var host = new HostBuilder()
                .ConfigureServices((hostContext, services) => ConfigureServices(services)) // Задаем конфигурацию
                .UseConsoleLifetime() // Позволяет поддерживать приложение активным в консоли
                .Build(); // Собираем

            Console.WriteLine("service started");
            // Запускаем сервис
            await host.RunAsync();
            Console.WriteLine("service stopped");
        }

        static void ConfigureServices(IServiceCollection services)
        {
            // Регистрируем объект TelegramBotClient c токеном подключения
            //services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient(GetTokenString(@"/Users/user/source/repos/VoiceTexterBot/token.txt")));
            services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient(GetTokenString("/home/u/Документы/Telegram/token.txt")));
            
            // Регистрируем постоянно активный сервис бота
            services.AddHostedService<Bot>();
        }
        private static string GetTokenString(string path)
        {
            var sr = File.OpenText(path);
            string strToken = sr.ReadLine();
            return strToken;
        }
    }
}