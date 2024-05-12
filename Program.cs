using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
//using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Timers;
using System.Security.Cryptography;

class Program
{
    private static readonly TelegramBotClient botClient = new TelegramBotClient("7076051969:AAEfAB8T-uO8gBkawRPQZvlQTGnyQhDBbaQ");
    private static int botState = 0;
    private static int length = 8;
    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string NumericChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*()_-+=<>?/{}[]|";
    private static int number;
    private static string charSet = "";
    private static bool useLowercase;
    private static bool useUppercase;
    private static bool useNumeric;
    private static bool useSpecial;

    static async Task Main()
    {
        using CancellationTokenSource cts = new CancellationTokenSource();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        ReceiverOptions receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
        };

        botClient.StartReceiving(
            HandleUpdateAsync,
            HandlePollingErrorAsync,
            receiverOptions,
            cts.Token
        );

        var me = await botClient.GetMeAsync();

        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();

        // Send cancellation request to stop bot
        cts.Cancel();
    }

    static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;
        //var message = update.Message;

        if (message is null || message.Type != MessageType.Text)
            return;


        if (message.Text.StartsWith("/start"))
        {
            await SendMessageWithDelay(message.Chat, "Для генерации пароля укажите следующие параметры ->");
            await SendMessageWithDelay(message.Chat, "1. Укажите длину пароля:");
            botState = 1;
            charSet = "";
            return;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        if (botState == 1)
        {
            if (int.TryParse(message.Text, out number) && number > 5)
            {
                length = number;
                await SendMessageWithDelay(message.Chat, $"Длина пароля: {length}");
                await SendMessageWithDelay(message.Chat, "2. Использовать строчные буквы? (Да/Нет)");
                botState = 2;
                return;
            }      
            else
            {
                await SendMessageWithDelay(message.Chat, "Рекомендую использовать длину пароля 6 и больше");
                return;
            }
        }
        /////////////////////////////////////////////////////////////////////////////////////////
        if (botState == 2)
        {
            if (message.Text.ToLower() == "да")
            {
                useLowercase = true;
                charSet += LowercaseChars;
            }
            else if (message.Text.ToLower() != "нет")
            {
                useLowercase = false;
                await SendMessageWithDelay(message.Chat, "Используйте только слово 'Да' или 'Нет'");
                return;
            }
            await SendMessageWithDelay(message.Chat, "3. Использовать заглавные буквы? (Да/Нет)");
            botState = 3;
            return;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        if (botState == 3)
        {
            if (message.Text.ToLower() == "да")
            {
                useUppercase = true;
                charSet += UppercaseChars;
            }
            else if (message.Text.ToLower() != "нет")
            {
                useUppercase = false;
                await SendMessageWithDelay(message.Chat, "Используйте только слово 'Да' или 'Нет'");
                return;
            }
            await SendMessageWithDelay(message.Chat, "4. Использовать цифры? (Да/Нет)");
            botState = 4;
            return;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        if (botState == 4)
        {
            if (message.Text.ToLower() == "да")
            {
                useNumeric = true;
                charSet += NumericChars;
            }
            else if (message.Text.ToLower() != "нет")
            {
                useNumeric = false;
                await SendMessageWithDelay(message.Chat, "Используйте только слово 'Да' или 'Нет'");
                return;
            }
            await SendMessageWithDelay(message.Chat, "5.спользовать специальные символы ? (Да / Нет)");
            botState = 5;
            return;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        if (botState == 5)
        {
            if (message.Text.ToLower() == "да")
            {
                useSpecial = true;
                charSet += SpecialChars;
            }
            else if (message.Text.ToLower() != "нет")
            {
                useSpecial = false;
                await SendMessageWithDelay(message.Chat, "Используйте только слово 'Да' или 'Нет'");
                return;
            }

            var password = "";

            if (string.IsNullOrEmpty(charSet))
            { 
                //throw new ArgumentException("At least one character set must be selected for password generation.");
                await SendMessageWithDelay(message.Chat, "Вами не был выбран ни один параметр.\nПоэтому будет сгенерирован стандартный пароль:");
                charSet += LowercaseChars;
                charSet += UppercaseChars;
                charSet += NumericChars;
                useSpecial = false;
                useNumeric = true;
                useUppercase = true;
                useLowercase = true;

            }

            using (var rng = RandomNumberGenerator.Create())
            {
                var buffer = new byte[sizeof(int)];
                for (var i = 0; i < length; i++)
                {
                    rng.GetBytes(buffer);
                    var randomNumber = BitConverter.ToUInt32(buffer, 0);
                    var randomIndex = (int)(randomNumber % charSet.Length);
                    password += charSet[randomIndex];
                }
            }
            //await SendMessageWithDelay(message.Chat, $"charSet: {charSet}");
            //await SendMessageWithDelay(message.Chat, $"Параметры пароля:\nСтрочные буквы {useLowercase}\nЗаглавные буквы {useUppercase}\nЦифры {useNumeric}\nСпециальные символы {useSpecial}");
            await SendMessageWithDelay(message.Chat, 
                $"Параметры пароля:\n1. Длина: {length}\n"
                + $"2. Строчные буквы: {(useLowercase ? "\u2705" : "\u274C")}\n"
                + $"3. Заглавные буквы: {(useUppercase ? "\u2705" : "\u274C")}\n"
                +$"4. Цифры: {(useNumeric ? "\u2705" : "\u274C")}\n"
                +$"5. Специальные символы: {(useSpecial ? "\u2705" : "\u274C")}");
            await SendMessageWithDelay(message.Chat, $"Пароль:\n{password}");
            await SendMessageWithDelay(message.Chat, "Что бы сгенерировать новый пароль нажмите /start");
            //await SendMessageWithDelay(message.Chat, $"charSet: {charSet}");
            botState = 6;
            return;
        }
    }

    static async Task SendMessageWithDelay(ChatId chatId, string text)
    {
        await Task.Delay(500);
        await botClient.SendTextMessageAsync(chatId, text);
    }

    static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}
