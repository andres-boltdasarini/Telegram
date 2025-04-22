using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace VoiceTexterBot
{
    internal class Bot : BackgroundService
    {
        private ITelegramBotClient _telegramClient;

        public Bot(ITelegramBotClient telegramClient)
        {
            _telegramClient = telegramClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _telegramClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions() { AllowedUpdates = { } },
                cancellationToken: stoppingToken);

            Console.WriteLine("Bot started");
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, 
            CancellationToken cancellationToken)
        {
            //  Telegram Bot API: https://core.telegram.org/bots/api#callbackquery
            if (update.Type == UpdateType.CallbackQuery)
            {
                await _telegramClient.SendMessage(update.CallbackQuery.From.Id, $"err send text.",
                    cancellationToken: cancellationToken);
                return;
            }

            // Telegram Bot API: https://core.telegram.org/bots/api#message
            if (update.Type == UpdateType.Message)
            {
                double total =0;
                Console.WriteLine($"text  {update.Message.Text}");
                if (double.TryParse(update.Message.Text, out double value))
                {
                    total = Calcul(update.Message.Text);
                }
                    await _telegramClient.SendMessage(update.Message.Chat.Id, $"text Length: {update.Message.Text.Length} num sum {total}", cancellationToken: cancellationToken);
                return;
            }
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };


            Console.WriteLine(errorMessage);


            Console.WriteLine("Waiting 10 seconds before retry");
            Thread.Sleep(10000);

            return Task.CompletedTask;
        }
        private double Calcul(string str) 
        {
            Console.WriteLine($"this is num");
            List<double> nums = new List<double>();
            string num = "";
            double db = 0;
            foreach (char ch in str)
            {
                if (ch != ' ')
                {
                    num += ch;
                }
                else
                {
                    db = double.Parse(num);
                    nums.Add(db);
                    num = "";
                }
            }
            db = double.Parse(num);
            nums.Add(db);
            return nums.Sum();
        }
    }
}