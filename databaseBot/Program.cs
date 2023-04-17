using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.IO;

//Custom Classes
// using CommandHandlerClass;

namespace CosmicBot
{
    public class Program
    {
        public static Task Main(string[] args) => new Program().MainAsync();

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private DiscordSocketClient _client;
        public async Task MainAsync()
        {
            // This activates the Bot on discord and authorizes it via the token 
            _client = new DiscordSocketClient();
            _client.Log += Log;
            //  You can assign your bot token to a string, and pass that in to connect.
            //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
            //var token = "token"; //Should only be used for testing purposes, otherwise the other method is more secure
            var token = File.ReadAllText("token.txt");

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            //Blocks this task until the program is closed
            await Task.Delay(-1);
        }

        public class CommandHandler
        {
            private readonly DiscordSocketClient _client;
            private readonly CommandService _commands;

            //Retireve client and command service instance via ctor
            public CommandHandler(DiscordSocketClient client, CommandService commands)
            {
                _commands = commands;
                _client = client;
            }

            public async Task InstallCommandsAsync()
            {
                //Hook the Message received event into our command handler
                _client.MessageReceived += HandleCommandAsync;
                // Here we discover all of the command modules in the entry 
                // assembly and load them. Starting from Discord.NET 2.0, a
                // service provider is required to be passed into the
                // module registration method to inject the 
                // required dependencies.
                //
                // If you do not use Dependency Injection, pass null.
                // See Dependency Injection guide for more information.
                await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: null);
            }

            private async Task HandleCommandAsync(SocketMessage messageParam)
            {
                // Dont process the command if it was a system Message
                var message = messageParam as SocketUserMessage;
                if (message == null) return;

                //Create a number to track where the prefix ends and the command begins
                int argPos = 0;

                //Determine if the message is a command based on the prefix and make sure no bots trigger commands 
                if (!(message.HasCharPrefix('_', ref argPos) ||
                    message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                    message.Author.IsBot)
                    return;

                //Create a websocket based command context based on the message
                var context = new SocketCommandContext(_client, message);

                //Execture the command with the command context
                //along with te service provider for precondition checks
                await _commands.ExecuteAsync(
                    context: context,
                    argPos: argPos,
                    services: null);
            }

        }
    }
}
