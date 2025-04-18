using Telegram.Bot;
using Telegram.Bot.Types;

namespace VoiceTexterBot.Controllers
{
    internal class InlineKeyboardController
    {
        private readonly ITelegramBotClient _telegramClient;

        public InlineKeyboardController(ITelegramBotClient telegramBotClient)
        {
            _telegramClient = telegramBotClient;
        }
        public async Task Handle(CallbackQuery? callbackQuery, CancellationToken ct)
        {
            Console.WriteLine($"Контроллер {GetType().Name} обнаружил нажатие на кнопку");

            await _telegramClient.SendMessage(callbackQuery.From.Id, $"Обнаружено нажатие на кнопку", cancellationToken: ct);
        }
    }
}

