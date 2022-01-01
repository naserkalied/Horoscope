using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;


namespace ZodiacBot
{
    class Program
    {
        private static TelegramBotClient? Bot;

        public static async Task Main()
        {
            Bot = new TelegramBotClient("5083784168:AAGoWATWP3ejBG3cRPDEzctR3Ce4dcD9npw");

            User me = await Bot.GetMeAsync();
            Console.Title = me.Username ?? "My awesome Bot";

            using var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            Bot.StartReceiving(HandleUpdateAsync,
                               HandleErrorAsync,
                               receiverOptions,
                               cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
        }

        /////////////////////////////////////
        ///

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;


            var action = message.Text switch
            {
                "/keyboard" => SendReplyKeyboard(botClient, message),
                "/help" => help(botClient, message),
                "Aries" or "aries" or "Taurus" or "taurus" or "Gemini" or "gemini" or "Cancer" or "cancer"
                 or "Leo" or "leo" or "Virgo" or "virgo" or "Libra" or "libra" or "Scorpio" or "scorpio"
                 or "Sagittarius" or "sagittarius" or "Capricorn" or "capricorn" or "Aquarius" or "aquarius" or "Pisces" or "pisces"
                 => replyToQ1(botClient, message),
                _ => Usage(botClient, message)
            };
            Message sentMessage = await action;
            Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");


            static async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message message)
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(
                    new[]
                    {
                        new KeyboardButton[] { "Aries", "Taurus", "Gemini", "Cancer" },
                        new KeyboardButton[] { "Leo", "Virgo", "Libra", "Scorpio" },
                        new KeyboardButton[] { "Sagittarius", "Capricorn", "Aquarius", "Pisces" },
                    })
                {
                    ResizeKeyboard = true
                };

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Choose",
                                                            replyMarkup: replyKeyboardMarkup);
            }


            static async Task<Message> replyToQ1(ITelegramBotClient botClient, Message message)
            {
                string[] receivedMsg = SelectZodiac(message.Text);

                return await botClient.SendVideoAsync(
                    chatId: message.Chat.Id,
                    video: receivedMsg[1],
                    supportsStreaming: true,
                    caption: receivedMsg[0] + "\n" + "<i>Source</i>: <a href=\"https://blog.prepscholar.com\">prepscholar</a>",
                    parseMode: ParseMode.Html,
                    replyToMessageId: message.MessageId,
                    replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithUrl(
                            "Click for more info",
                            "https://en.wikipedia.org/wiki/Zodiac"))
                        );

            }



            static async Task<Message> help(ITelegramBotClient botClient, Message message)
            {

                Message rMessage = await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Type the name of the zodiac to get information about it\n" +
                    "or use the internal keyboard 😜\n"
                        );

                return await Usage(botClient, message);
            }



            static async Task<Message> Usage(ITelegramBotClient botClient, Message message)
            {
                const string usage = "Usage:\n" +
                                     "/keyboard - Work with keyboard\n" +
                                     "/help   - Get help";

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: usage,
                                                            replyMarkup: new ReplyKeyboardRemove());
            }
        }


        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }

        /////////////////////////////////////
        ///
        static string[] SelectZodiac(string s)
        {
            string[] Localmsg = { "", "" };
            switch (s)
            {
                case "Aries" or "aries":
                    Localmsg[0] = "Aries is the first of the twelve zodiac signs and is represented by the constellation, the ram. If born under this sign, you're considered to be adventurous, dynamic, ambitious and competitive. Aries are known for their quickness and leadership qualities, as well as a tendency to be impulsive (blame the fire element) and blunt.";
                    Localmsg[1] = "https://media.giphy.com/media/RMePWGZwvngc6ebdCw/giphy.gif";
                    break;
                case "Taurus" or "taurus":
                    Localmsg[0] = "Taurus is the second of the twelve zodiac signs and is represented by the constellation, Taurus. If born under this sign, you're considered to be dedicated, dependable, focused and creative. Tauruses are known for being intelligent and trustworthy, as well as stubborn (the sign is a bull, after all). Tauruses love to seek out pleasure and can be known to question authority.";
                    Localmsg[1] = "https://media.giphy.com/media/lSz3J36JVDG4bTWMME/giphy.gif";
                    break;
                case "Gemini" or "gemini":
                    Localmsg[0] = "Gemini is the third of the twelve zodiac signs and is represented by the constellation Gemini, which is made up of the Dioscuri—the twins, Castor and Pollux. If born under this sign, you're considered to be energetic, expressive, intelligent and playful. Gemini are known for their outgoing nature and varied interests, but they have earned a (likely misplaced) reputation for being two-faced.";
                    Localmsg[1] = "https://media.giphy.com/media/ViOMR3f1z9mq3MELJm/giphy.gif";
                    break;
                case "Cancer" or "cancer":
                    Localmsg[0] = "Cancer is the fourth of the twelve zodiac signs and is represented by the constellation, Cancer, which is most often depicted as a crab. If born under this sign, you're considered to be bold, compassionate, protective and intuitive. Cancers are known for their care-giving nature, as well as a tendency to be distant and passive-aggressive.";
                    Localmsg[1] = "https://media.giphy.com/media/dZpd8u44q3mDIe8VO9/giphy.gif";
                    break;
                case "Leo" or "leo":
                    Localmsg[0] = "Leo is the fifth of the twelve zodiac signs and is represented by the constellation, the lion. If born under this sign, you're considered to be vivacious, outgoing and fiery. Leos are known for their warm nature and high self-esteem, but they can have a tendency to be proud or jealous.";
                    Localmsg[1] = "https://media.giphy.com/media/Q8ZruFpT4VGVe2qFbE/giphy.gif";
                    break;
                case "Virgo" or "virgo":
                    Localmsg[0] = "Virgo is the sixth of the twelve zodiac signs and is represented by the constellation, the maiden. If born under this sign, you're considered to be practical, analytical and sophisticated. Virgos are known for their kindness and attention to detail, but they can have a tendency to be shy and have unfairly high standards for themselves and their loved ones.";
                    Localmsg[1] = "https://media.giphy.com/media/cn3dcc98ZOhjZPGQSC/giphy.gif";
                    break;
                case "Libra" or "libra":
                    Localmsg[0] = "Libra is the seventh of the twelve zodiac signs and is represented by the only inanimate constellation, the scales. If born under this sign, you're considered to be balanced, social and diplomatic. Libras are known for their selfless nature and companionship, but they can have a tendency to be too pragmatic and insecure.";
                    Localmsg[1] = "https://media.giphy.com/media/VgNctFELbNwdTNMWRs/giphy.gif";
                    break;
                case "Scorpio" or "scorpio":
                    Localmsg[0] = "Scorpio is the eighth of the twelve zodiac signs and is represented by the constellation, the scorpion. If born under this sign, you're considered to be loyal, resourceful and focused. Scorpios are known for their bravery and trailblazing nature, but they can appear prickly and closed off to strangers.";
                    Localmsg[1] = "https://media.giphy.com/media/VgGrCd14gy8NSBX2mC/giphy.gif";
                    break;
                case "Sagittarius" or "sagittarius":
                    Localmsg[0] = "Sagittarius is the ninth of the twelve zodiac signs and is represented by the constellation, the archer. If born under this sign, you're considered to be optimistic, independent and intellectual. Sagittariuses are known for being magnetic and generous, but they can have a tendency to be arrogant and too direct.";
                    Localmsg[1] = "https://media.giphy.com/media/H7fYQ9eS9uGjKwurBd/giphy.gif";
                    break;
                case "Capricorn" or "capricorn":
                    Localmsg[0] = "Capricorn is the tenth of the twelve zodiac signs and is represented by the constellation, the sea goat. If born under this sign, you're considered to be patient, hardworking and disciplined. Capricorns are known for their tenacity and preference for boundaries and rules, but they can have a tendency to be stubborn and too focused on perfection.";
                    Localmsg[1] = "https://media.giphy.com/media/dutzEo8JsuwbMrahJH/giphy.gif";
                    break;
                case "Aquarius" or "aquarius":
                    Localmsg[0] = "Aquarius is the eleventh of the twelve zodiac signs and is represented by the constellation, the water bearer. If born under this sign, you're considered to be innovative, loyal and original. Aquariuses are known for their creativity and rebellious nature, but they can have a tendency to be aloof with loved ones and uncompromising.";
                    Localmsg[1] = "https://media.giphy.com/media/kEbn3fvRtNtZUkjfYA/giphy.gif";
                    break;
                case "Pisces" or "pisces":
                    Localmsg[0] = "Pisces is the final of the twelve zodiac signs and is represented by the constellation, the fishes. If born under this sign, you're considered to be intuitive, creative and empathetic. Pisces are known for their compassion and artistic nature, but they can have a tendency to be too sensitive or delusional.";
                    Localmsg[1] = "https://media.giphy.com/media/YPPGJVeZOrBaIEiSYu/giphy.gif";
                    break;

            }
            return Localmsg;

        }


    }
}
