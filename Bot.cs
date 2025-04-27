using System.Threading;
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
        private ITelegramBotClient _telegramClient;
        public string State { get; set; }

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


            if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Text)
            {

                await HandleMessageAsync(update.Message, cancellationToken);
            }
            if (update.Type == UpdateType.CallbackQuery)
            {
                var data = update.CallbackQuery.Data;

                State = data;
                switch (update.CallbackQuery.Data)
                {
                    case $"sumCharLength":

                        await _telegramClient.SendMessage(update.CallbackQuery.From.Id, $"Отправьте текст для подсчёта символов" );

                        break;

                    default:
                        await _telegramClient.SendMessage(update.CallbackQuery.From.Id, $"Отправьте числа через пробел для вычисления суммы");

                        break;
                }

            }

        }
        public async Task HandleMessageAsync(Message message, CancellationToken ct)
        {
            if(State== "sumCharLength")

                {
                await _telegramClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: $"Количество символов: {message.Text.Length}");
                }
                else if (State == "calculateNum")
                {
                    try
                    {
                        var sum = Calcul(message.Text);
                    await _telegramClient.SendMessage(
                            chatId: message.Chat.Id,
                            text: $"Сумма чисел: {sum}");
                    }
                    catch
                    {
                    await _telegramClient.SendMessage(
                            chatId: message.Chat.Id,
                            text: "Ошибка: введите числа через пробел (например: 1 2 3)");
                    }
                }
            switch (message.Text)
            {
                case "/start":
                    var buttons = new List<InlineKeyboardButton[]>();
                    buttons.Add(new[]
                    {
                        InlineKeyboardButton.WithCallbackData($" Сумма символов в строке" , $"sumCharLength"),
                        InlineKeyboardButton.WithCallbackData($" Сумма чисел через пробел" , $"calculateNum")
                    });

                    // передаем кнопки вместе с сообщением (параметр ReplyMarkup)
                    await _telegramClient.SendMessage(message.Chat.Id, $"<b>  Наш бот превращает аудио в текст.</b> {Environment.NewLine}" +
                        $"{Environment.NewLine}Можно записать сообщение и переслать другу, если лень печатать.{Environment.NewLine}", cancellationToken: ct, parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(buttons));

                    break;

                default:
                    await _telegramClient.SendMessage(
                        message.Chat.Id,
                        "Отправьте аудио для превращения в текст.",
                        cancellationToken: ct);
                    break;
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