﻿using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

//Custom Classes go here
// using CommandsClass;

namespace CommandHandlerClass
{
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