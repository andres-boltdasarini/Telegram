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
                new ReceiverOptions() { AllowedUpdates = { } }, // ����� ��������, ����� ���������� ����� ��������. � ������ ������ ��������� ���
                cancellationToken: stoppingToken);

            Console.WriteLine("��� �������");
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, 
            CancellationToken cancellationToken)
        {
            //  ������������ ������� �� ������  �� Telegram Bot API: https://core.telegram.org/bots/api#callbackquery
            //if (update.Type == UpdateType.CallbackQuery)
            //{
            //    await _telegramClient.SendMessage(update.Message.Chat.Id, 
            //        $"����� ���������: {update.Message.Text.Length} ������", cancellationToken: cancellationToken);
            //    return;
            //}

            //������������ �������� ��������� �� Telegram Bot API: https://core.telegram.org/bots/api#message
            if (update.Type == UpdateType.Message)
            {
                Console.WriteLine($"�������� ��������� {update.Message.Text}");
                await _telegramClient.SendMessage(update.Message.Chat.Id, $"����� ���������: {update.Message.Text.Length} ������", cancellationToken: cancellationToken);
                return;
            }
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // ������ ��������� �� ������ � ����������� �� ����, ����� ������ ������ ���������
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            // ������� � ������� ���������� �� ������
            Console.WriteLine(errorMessage);

            // �������� ����� ��������� ������������
            Console.WriteLine("������� 10 ������ ����� ��������� ������������.");
            Thread.Sleep(10000);

            return Task.CompletedTask;
        }
    }
}