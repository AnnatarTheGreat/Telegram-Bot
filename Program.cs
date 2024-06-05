using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


var botClient = new TelegramBotClient("3257356732:RLYnGGaxDsoSimbnExt-TiMeXdqDQzy9MkE");
using CancellationTokenSource cts = new CancellationTokenSource();
using StreamWriter streamWriter= new StreamWriter("Results.txt");

ReceiverOptions receiverOptions = new ReceiverOptions()
{
    AllowedUpdates = Array.Empty<UpdateType>()
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();
Console.Clear();
Console.WriteLine("Start listening for messages!");
Console.ReadLine();
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    
    var replyMarkup = new ReplyKeyboardMarkup(
        new List<KeyboardButton[]>() {
            new KeyboardButton[] { new KeyboardButton("Receive random film") }
        }
    )
    { ResizeKeyboard = true };

    if (update.Message is not { } message)
        return;
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;
    var username = message.From.Username;
    var firstName = message.From.FirstName;
    var lastName = message.From.LastName;
    if (messageText == "/start")
    {
        await botClient.SendTextMessageAsync(
                                   chatId,
                                   "Push the button below to receive random film!",
                                   replyMarkup: replyMarkup);
    }

    if (messageText == "Receive random film")
    {
        using (var client = new HttpClient())
        {
            string film = await FindFilm(client, chatId);
            if (film != null)
            {
                Console.WriteLine($"{firstName} {username} {lastName} received {film} at {DateTime.Now}");
                streamWriter.WriteLine($"{firstName} {username} {lastName} received {film} at {DateTime.Now}");
                streamWriter.Flush();
            }
        }
    }
}

async Task<string> FindFilm(HttpClient client, long chatId)
{
    string showType = "";
    string filmName = "";
    string filmID = "";

    var random = new Random();
    while (showType != "movie")
    {
        int randomNumber = random.Next(1, 2299999);
        filmID = "tt" + randomNumber.ToString("D7");
        var response = await client.GetAsync($"http://www.omdbapi.com/?apikey=ofcItsKeyxd&i={filmID}");
        if (response.IsSuccessStatusCode)
        {
            string data = await response.Content.ReadAsStringAsync();
            var receivedObject = JsonSerializer.Deserialize<IMDBModel>(data);
            if (receivedObject == null)
            {
                await botClient.SendTextMessageAsync(
                chatId,
                $"There is some mistake. Try again!"
                );
                return null;
            }
            showType = receivedObject.Type;
            filmName = receivedObject.Title;
        }
        else
        {
            await botClient.SendTextMessageAsync(
            chatId,
            $"There is some mistake. Try again!"
            );
        }

    }

    await botClient.SendTextMessageAsync(
            chatId,
            $"https://www.imdb.com/title/{filmID}/"
            );

    return $"https://www.imdb.com/title/{filmID}/";
}















Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error: \n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}








