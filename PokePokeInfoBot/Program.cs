using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PokeApiNet;
using PokePokeInfoBot;
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

            if (input.Contains("/start"))
            {
            Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Hello \nPlease Enter just the pokemon name to get its details" ,
            cancellationToken: cancellationToken);

            }
        else
        {
            await FetchPokemonInfo(message.Chat.Id, input);
        }
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
        try
        {
            

                PokemonDetails creatureDetails = new PokemonDetails();
                var pokemon = creatureDetails.GetThisPokemon(pokemonName).Result;

            string message = $"Name: {pokemon.Name}\n" +
                             $"Weight: {pokemon.Weight}\n" +
                             $"Height: {pokemon.Height}\n\n";

            message += "Abilities:\n" + GetAbilitiesMessage(pokemon.Abilities) + "\n";
           // message += "Base Stats:\n" + GetStatsMessage(pokemon.Stats) + "\n";
            message += "Type: " + GetTypeMessage(pokemon.Types,creatureDetails);


            await botClient.SendTextMessageAsync(chatId, message);

        }
        catch (Exception ex)
        {
            await botClient.SendTextMessageAsync(chatId, $"An error occurred: {ex.Message}");
        }

    }

    // Extracted methods for better modularity

    private static string GetAbilitiesMessage(List<PokemonAbility> abilities)
    {
        return string.Join("\n", abilities.Select(ability => $"- {ability.Ability.Name}"));
    }

    private static string GetStatsMessage(List<PokemonStat> stats)
    {
        return string.Join("\n", stats.Select(stat => $"- {stat.Stat.Name}: {stat.BaseStat}"));
    }

    private static string GetTypeMessage(List<PokemonType> types, PokemonDetails creatureDetails)
    {
        var message = new StringBuilder();

        foreach (var type in types)
        {
            var typeDetails = creatureDetails.GetThisType(type.Type.Url).Result;
            message.AppendLine($"{type.Type.Name}");

            var damageRelations = typeDetails.DamageRelations;

            AppendDamageRelations(message, "Strong against (Takes No DMG)", damageRelations.NoDamageFrom);
            AppendDamageRelations(message, "Super Effective against (Deals Double DMG)", damageRelations.DoubleDamageTo);
            AppendDamageRelations(message, "Effective against (Half DMG to)", damageRelations.HalfDamageTo);
            AppendDamageRelations(message, "Ineffective Against (No DMG)", damageRelations.NoDamageTo);
            AppendDamageRelations(message, "Weak against (Half DMG From)", damageRelations.HalfDamageFrom);
            AppendDamageRelations(message, "Very Weak against (Double DMG From", damageRelations.DoubleDamageFrom);

        }

        return message.ToString();
    }

    private static void AppendDamageRelations(StringBuilder message, string label, List<NamedApiResource<PokeApiNet.Type>> relations)
    {
        if (relations.Count > 0)
        {
            message.AppendLine($"- {label}:");
            foreach (var relation in relations)
            {
                message.AppendLine($"  - {relation.Name}");
            }
        }
    }
}




