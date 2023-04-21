using Discord;
using Discord.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;
using DatabaseConnectionClass;
using System.Security.Cryptography.X509Certificates;
using Discord.Commands;

namespace CosmicBot
{
    // This is a minimal, bare-bones example of using Discord.Net.
    //
    // If writing a bot with commands/interactions, we recommend using the Discord.Net.Commands/Discord.Net.Interactions
    // framework, rather than handling them yourself, like we do in this sample.
    //
    // You can find samples of using the command framework:
    // - Here, under the TextCommandFramework sample
    // - At the guides: https://discordnet.dev/guides/text_commands/intro.html
    //
    // You can find samples of using the interaction framework:
    // - Here, under the InteractionFramework sample
    // - At the guides: https://discordnet.dev/guides/int_framework/intro.html
    class Program
    {
        DBConnect test = new DBConnect(); //allows for connections to the database to be made via the DatabaseConnection Class

        // Non-static readonly fields can only be assigned in a constructor.
        // If you want to assign it elsewhere, consider removing the readonly keyword.
        private readonly DiscordSocketClient _client;

        // Discord.Net heavily utilizes TAP for async, so we create
        // an asynchronous context from the beginning.
        static void Main(string[] args)
            => new Program()
                .MainAsync()
                .GetAwaiter()
                .GetResult();

        public Program()
        {
            // Config used by DiscordSocketClient
            // Define intents for the client
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            // It is recommended to Dispose of a client when you are finished
            // using it, at the end of your app's lifetime.
            _client = new DiscordSocketClient(config);


            // Subscribing to client events, so that we may receive them whenever they're invoked.
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
            _client.InteractionCreated += InteractionCreatedAsync;
        }

        public async Task MainAsync()
        {
            // Tokens should be considered secret data, and never hard-coded.
            var token = File.ReadAllText("token.txt");
            await _client.LoginAsync(TokenType.Bot, token);
            // Different approaches to making your token a secret is by putting them in local .json, .yaml, .xml or .txt files, then reading them on startup.

            await _client.StartAsync();

            // Block the program until it is closed.
            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        // The Ready event indicates that the client has opened a
        // connection and it is now safe to access the cache.
        private Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        // This is not the recommended way to write a bot - consider
        // reading over the Commands Framework sample.
        private async Task MessageReceivedAsync(SocketMessage message)
        {
            //variables
            string command = "";
            int lengthOfCommand = -1;

            //Filtering Messages
            if (!message.Content.StartsWith('_')) //This is the Prefix
                return;
            // The bot should never respond to itself.
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            if (message.Content.Contains(' '))
                lengthOfCommand = message.Content.IndexOf(' ');
            else
                lengthOfCommand = message.Content.Length;

            command = message.Content.Substring(1, lengthOfCommand - 1).ToLower();

            if (command.Equals("ping"))
            {
                // Create a new ComponentBuilder, in which dropdowns & buttons can be created.
                var cb = new ComponentBuilder()
                    .WithButton("Click me!", "unique-id", ButtonStyle.Primary);

                // Send a message with content 'pong', including a button.
                // This button needs to be build by calling .Build() before being passed into the call.
                await message.Channel.SendMessageAsync("pong!", components: cb.Build());
            }

            if (command.Equals("echo"))
            {
                string echo = " ";
                if (message.Content.Contains(""))
                {
                    echo = message.Content.Substring(5);
                    //Suppose to make it so the value is not null if the user doesnt add anything after the command
                    //Does not work currently
                    if(echo == " ")
                    {
                        echo = "Please type something after the command!";
                    }
                }
                //this is a test command for a test database custom insert from the user
                await message.Channel.SendMessageAsync(echo);

            }
        }

        // For better functionality & a more developer-friendly approach to handling any kind of interaction, refer to:
        // https://discordnet.dev/guides/int_framework/intro.html
        private async Task InteractionCreatedAsync(SocketInteraction interaction)
        {
            // safety-casting is the best way to prevent something being cast from being null.
            // If this check does not pass, it could not be cast to said type.
            if (interaction is SocketMessageComponent component)
            {
                // Check for the ID created in the button mentioned above.
                if (component.Data.CustomId == "unique-id")
                    await interaction.RespondAsync("Thank you for clicking my button!");

                else
                    Console.WriteLine("An ID has been received that has no handler!");
            }
        }
    }
}