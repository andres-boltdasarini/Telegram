using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace VoiceTexterBot
{
    internal class Bot : BackgroundService
    {
        private readonly ITelegramBotClient _telegramClient;
        private ILogger<Bot> _logger;
        public string State { get; set; }

        public Bot(ITelegramBotClient telegramClient, ILogger<Bot> logger)
        {
            _telegramClient = telegramClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _telegramClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions() { AllowedUpdates = { } },
                cancellationToken: stoppingToken);

            _logger.LogInformation("Bot started");
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Type == MessageType.Text)
            {
                await HandleMessageAsync(update.Message, cancellationToken);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                var data = update.CallbackQuery?.Data;

                if (data == null) return;

                State = data;
                switch (data)
                {
                    case "sumCharLength":
                        await _telegramClient.SendMessage(
                            update.CallbackQuery.From.Id,
                            "Отправьте текст для подсчёта символов",
                            cancellationToken: cancellationToken);
                        break;

                    default: // "calculateNum"
                        await _telegramClient.SendMessage(
                            update.CallbackQuery.From.Id,
                            "Отправьте числа через пробел для вычисления суммы",
                            cancellationToken: cancellationToken);
                        break;
                }
            }
        }

        public async Task HandleMessageAsync(Message message, CancellationToken ct)
        {
            if (message.Text == null) return;

            if (State == "sumCharLength")
            {
                await _telegramClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: $"Количество символов: {message.Text.Length}",
                    cancellationToken: ct);
            }
            else if (State == "calculateNum")
            {
                try
                {
                    var sum = CalculateSum(message.Text);
                    await _telegramClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: $"Сумма чисел: {sum}",
                        cancellationToken: ct);
                }
                catch
                {
                    await _telegramClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Ошибка: введите числа через пробел (например: 1 2 3)",
                        cancellationToken: ct);
                }
            }
            else
            {
                switch (message.Text)
                {
                    case "/start":
                        var buttons = new List<InlineKeyboardButton[]>();
                        buttons.Add(new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Сумма символов в строке", "sumCharLength"),
                            InlineKeyboardButton.WithCallbackData("Сумма чисел через пробел", "calculateNum")
                        });

                        await _telegramClient.SendMessage(
                            message.Chat.Id,
                            "Выберите действие",
                            cancellationToken: ct,
                            parseMode: ParseMode.Html,
                            replyMarkup: new InlineKeyboardMarkup(buttons));
                        break;

                    default:
                        await _telegramClient.SendMessage(
                            message.Chat.Id,
                            "Выберете start, чтобы начать.",
                            cancellationToken: ct);
                        break;
                }
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

            _logger.LogError(errorMessage);
            _logger.LogInformation("Waiting 10 seconds before retry");
            Thread.Sleep(10000);

            return Task.CompletedTask;
        }

        private double CalculateSum(string str)
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