using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class Bot
{
    private static TelegramBotClient botClient = new TelegramBotClient("7230220798:AAG9qmaY4PmizSw3Sg7xBcyOeMcqV0sjyts");
    private static string channelUsername = "@HusanjonBlog"; // Bu yerda kanalingiz username'ini kiriting

    public static async Task Main()
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // Receive all update types
        };

        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions);
        Console.WriteLine("Bot ishga tushdi");
        Console.ReadLine();
    }

    static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message)
        {
            var message = update.Message!;
            var chatId = message.Chat.Id;

            // Agar foydalanuvchi contact yuborsa
            
            if (message.Contact != null)
            {
                // Contact ma'lumotlari qabul qilindi, endi foydalanuvchiga xabar yuboramiz
                await botClient.SendTextMessageAsync(chatId, "Rahmat! Contact ma'lumotlaringiz qabul qilindi.");

                // Contact ma'lumotlari qabul qilindi, endi foydalanuvchidan ismini so'raymiz
                await botClient.SendTextMessageAsync(chatId, " Endi ismingizni kiriting.");
                
            }
            // Agar foydalanuvchi ism yuborsa
            else if (message.Type == MessageType.Text)
            {
                var messageText = message.Text;

                if (messageText == "/start")
                {
                    await botClient.SendTextMessageAsync(chatId, "Botga xush kelibsiz!");
                    await botClient.SendTextMessageAsync(chatId,"keling birinchi siz bilan tanishib olsak :)");
                    // Contact so'rash uchun tugma
                    var requestContactButton = new KeyboardButton("Contact yuborish") { RequestContact = true };
                    var keyboard = new ReplyKeyboardMarkup(new[] { requestContactButton }) { ResizeKeyboard = true };
                   
                    // Contact yuborish tugmasini olib tashlash
                    var removeKeyboard = new ReplyKeyboardRemove();
                    
                    await botClient.SendTextMessageAsync(chatId, "Iltimos, contact yuboring", replyMarkup: keyboard);
                }
                else
                {
                    // Foydalanuvchi ism kiritadi
                    if (!string.IsNullOrEmpty(messageText))
                    {
                        await botClient.SendTextMessageAsync(chatId, $"Rahmat! Ismingiz qabul qilindi: {messageText}");

                        // Kanalga obuna bo'lish uchun tugma
                        var channelLink = $"https://t.me/{channelUsername.Replace("@", "")}";
                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithUrl("Kanalga obuna bo'lish", channelLink),
                            InlineKeyboardButton.WithCallbackData("Obuna bo'lishni tekshirish", "check_subscription")
                        });
                        await botClient.SendTextMessageAsync(chatId, $"Iltimos, {channelUsername} kanaliga obuna bo'ling va 'Obuna bo'lishni tekshirish' tugmasini bosing.", replyMarkup: inlineKeyboard);
                    }
                    else
                    {
                        // Foydalanuvchi ism kiritmagan bo'lsa, yana so'rash
                        await botClient.SendTextMessageAsync(chatId, "Iltimos, ism kiriting. Ism majburiy!");
                    }
                }
            }
        }
        // Agar foydalanuvchi inline tugmani bossa (callback query)
        else if (update.Type == UpdateType.CallbackQuery)
        {
            var callbackQuery = update.CallbackQuery;
            if (callbackQuery!.Data == "check_subscription")
            {
                // Foydalanuvchining obuna bo'lganligini tekshirish
                bool isSubscribed = await CheckIfUserIsSubscribedToChannel(botClient, callbackQuery.From.Id);

                if (isSubscribed)
                {
                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Siz obuna bo'ldingiz!", showAlert: true);
                    await botClient.SendTextMessageAsync(callbackQuery.Message!.Chat.Id, "Siz kanalga obuna bo'lgansiz. Ma'lumotlar bilan tanishishingiz mumkin.");
                }
                else
                {
                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Siz hali obuna bo'lmagansiz. Iltimos, obuna bo'ling!", showAlert: false);
                    await botClient.SendTextMessageAsync(callbackQuery.Message!.Chat.Id, $"Iltimos, {channelUsername} kanaliga obuna bo'ling va yana urinib ko'ring.");
                }
            }
        }
    }

    // Foydalanuvchini kanalga obuna bo'lganligini tekshirish funksiyasi
    static async Task<bool> CheckIfUserIsSubscribedToChannel(ITelegramBotClient? botClient, long userId)
    {
        try
        {
            var chatMember = await botClient.GetChatMemberAsync(channelUsername, userId);
            // Agar foydalanuvchi kanalga a'zo bo'lsa (member yoki administrator bo'lsa), obuna bo'lgan hisoblanadi
            if (chatMember.Status == ChatMemberStatus.Member || chatMember.Status == ChatMemberStatus.Administrator || chatMember.Status == ChatMemberStatus.Creator)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Kanal a'zoligini tekshirishda xato: {ex.Message}");
        }
        return false;
    }

    static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Xato yuz berdi: {exception.Message}");
        return Task.CompletedTask;
    }
}