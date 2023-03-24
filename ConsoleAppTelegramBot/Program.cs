using ConsoleAppTelegramBot.Models;
using System.Net.Http.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

string valyutaApi = "https://cbu.uz/uz/arkhiv-kursov-valyut/json/";

HttpClient httpClient = new HttpClient();
var response = await httpClient.GetAsync(valyutaApi);

Valyuta[] valyutalar = await response.Content.ReadFromJsonAsync<Valyuta[]>();

ITelegramBotClient client = new TelegramBotClient("5736306328:AAGt_l0CqHayvVVQzqStMpbosESedQTLZG8");

List<User> users = new List<User>();

var inlineKeyBtn = new List<List<InlineKeyboardButton>>();

for (int i = 0; i < valyutalar.Length; i += 3)
{
    var inkb = new List<InlineKeyboardButton>();
    for (int j = i; j < i + 5 && j < valyutalar.Length; j++)
    {
        inkb.Add(
            InlineKeyboardButton
            .WithCallbackData(
                text: valyutalar[j].Ccy,
                callbackData: valyutalar[j].Code));
    }
    inlineKeyBtn.Add(inkb);
}
Console.WriteLine(valyutalar.Length);
var menyuMarkup = new InlineKeyboardMarkup(inlineKeyBtn);


client.OnMessage += Client_OnMessage;

client.OnCallbackQuery += Client_OnCallbackQuery;



client.StartReceiving();

Console.WriteLine("Bot ishga tushdi ... ");

Console.ReadKey();


async void Client_OnCallbackQuery(object? sender, CallbackQueryEventArgs e)
{
    string data = e.CallbackQuery.Data;
    long chatId = e.CallbackQuery.From.Id;

    if (valyutalar.Any(v => v.Code.ToString() == data))
    {
        var valyuta = valyutalar.First(v => v.Code.ToString() == data);
        await client.SendTextMessageAsync(chatId,
            $"<b>1</b> {valyuta.CcyNm_UZ} = <b>{valyuta.Rate}</b> uz so'miga teng!\n"
            + $"Oxirgi yangilanish: <b>{valyuta.Date}</b>",
            ParseMode.Html);
    }
}


async void Client_OnMessage(object? sender, MessageEventArgs e)
{

    long chatId = e.Message.Chat.Id;
    if (e.Message.Type == MessageType.Contact)
    {

        if (!users.Any(u => u.Tel == e.Message.Contact.PhoneNumber))
        {
            users.Add(new User
            {
                Id = chatId,
                Name = e.Message.Chat.FirstName,
                Tel = e.Message.Contact.PhoneNumber
            });

            ReplyKeyboardMarkup menyu = new ReplyKeyboardMarkup();
            menyu.Keyboard = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Menyu")
                }
            };
            menyu.ResizeKeyboard=true;
           

            await client.SendTextMessageAsync(chatId,
            "Tabriklaymiz botimizdan ro'yhatdan o'tdingiz!",
            replyMarkup: menyu);

            await client.SendTextMessageAsync(chatId,
           "Iltimos kerakli menyuni tanlang!",
           replyMarkup: menyuMarkup);
        }
        else
        {
            await client.SendTextMessageAsync(chatId,
                "Bu foydalanuvchi avval ro'yhatdan o'tgan!\n" +
                "Iltimos qatadan /start bosing!");
        }
        return;
    }

    if (e.Message.Type != MessageType.Text)
    {
        await client.SendTextMessageAsync(chatId,
            $"Kechirasiz bot faqat matnli xabarlarga javob beradi.");
        return;
    }

    string msg = e.Message.Text;

    if (msg.Equals("/start"))
    {
        ReplyKeyboardMarkup markup = new ReplyKeyboardMarkup(
            KeyboardButton.WithRequestContact("Telefon raqamni yuborish."));
        markup.ResizeKeyboard = true;

        await client.SendTextMessageAsync(chatId,
            $"Assalomu aleykum hurmatli <b>{e.Message.Chat.FirstName}</b> bitimizga xush kelibsiz!\n"
            + "Botdan foydalanish uchun telefon raqamizni yuboring!",
            ParseMode.Html,
            replyMarkup: markup);
        return;
    }

    if (!users.Any(u => u.Id == chatId))
    {
        ReplyKeyboardMarkup markup = new ReplyKeyboardMarkup(
            KeyboardButton.WithRequestContact("Telefon raqamni yuborish."));
        markup.ResizeKeyboard = true;

        await client.SendTextMessageAsync(chatId,
           "Avval botdan ro'yhatdan o'ting.",
           replyMarkup: markup);
        return;
    }


    if (valyutalar.Any(v => v.CcyNm_UZ == msg))
    {
        var valyuta = valyutalar.First(v => v.CcyNm_UZ == msg);
        await client.SendTextMessageAsync(chatId,
            $"<b>1</b> {valyuta.CcyNm_UZ} = <b>{valyuta.Rate}</b> uz so'miga teng!\n"
            + $"Oxirgi yangilanish: <b>{valyuta.Date}</b>",
            ParseMode.Html);
    }
    else
    {
        await client.SendTextMessageAsync(chatId,
            "Iltimos kerakli menyuni tanlang!",
            replyMarkup: menyuMarkup);
    }

}