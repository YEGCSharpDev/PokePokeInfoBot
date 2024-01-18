using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using PokeApiNet;
using System.Reflection;

namespace PokePokeInfoBot
{
    internal class PokemonDetails
    {
        PokeApiClient pokeClient = new PokeApiClient();

        public async Task <Pokemon> GetThisPokemon(string pokemonName)
        {

            return await pokeClient.GetResourceAsync<Pokemon>(pokemonName);
        }

        public async Task<PokeApiNet.Type> GetThisType(string pokeURI)
        {
            var urlElements = pokeURI.Split('/');
            var a = urlElements[(urlElements.Length) - 2];
            try
            {
                return await pokeClient.GetResourceAsync<PokeApiNet.Type>(int.Parse(urlElements[(urlElements.Length)-2]));
            }
            catch (Exception)
            {

                throw;
            }
            
        }



    }
}
