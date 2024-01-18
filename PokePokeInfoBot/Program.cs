using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

class Program
{
    private static TelegramBotClient botClient;
    private static readonly string Token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");

    static async Task Main()
    {
        if (string.IsNullOrEmpty(Token))
        {
            Console.WriteLine("Telegram bot token is missing. Set the TELEGRAM_BOT_TOKEN environment variable.");
            return;
        }

        botClient = new TelegramBotClient(Token);

        using CancellationTokenSource cts = new();

        
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>() 
        };

        botClient.StartReceiving(
            updateHandler: Bot_OnMessage,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );
        Console.WriteLine("Bots running");
        Console.ReadLine();
    }

    private static async Task Bot_OnMessage(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return;
        // Only process text messages
        if (message.Text is not { } messageText)
            return;

            string input = message.Text.ToLower();

            await FetchPokemonInfo(message.Chat.Id, input);
  

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


    private static async Task FetchPokemonInfo(long chatId, string pokemonName)
    {
        string apiUrl = $"https://pokeapi.co/api/v2/pokemon/{pokemonName}";
        HttpClient client = new HttpClient();

        try
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                PokemonData pokemon = JsonConvert.DeserializeObject<PokemonData>(json);

                string message = $"Name: {pokemon.Name}\n" +
                                 $"Weight: {pokemon.Weight}\n" +
                                 $"Height: {pokemon.Height}\n\n" +
                                 "Abilities:\n";
                foreach (var ability in pokemon.Abilities)
                {
                    message += $"- {ability.Ability.Name}\n";
                }

                message += "\nBase Stats:\n";
                foreach (var stat in pokemon.Stats)
                {
                    message += $"- {stat.Stat.Name}: {stat.BaseStat}\n";
                }

                await botClient.SendTextMessageAsync(chatId, message);
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, $"Failed to fetch data. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            await botClient.SendTextMessageAsync(chatId, $"An error occurred: {ex.Message}");
        }
        finally
        {
            client.Dispose();
        }
    }
}

public class PokemonData
{
    public string Name { get; set; }
    public int Weight { get; set; }
    public int Height { get; set; }
    public List<AbilityInfo> Abilities { get; set; }
    public List<StatInfo> Stats { get; set; }
}

public class AbilityInfo
{
    public Ability Ability { get; set; }
}

public class Ability
{
    public string Name { get; set; }
}

public class StatInfo
{
    public int BaseStat { get; set; }
    public Stat Stat { get; set; }
}

public class Stat
{
    public string Name { get; set; }
}
