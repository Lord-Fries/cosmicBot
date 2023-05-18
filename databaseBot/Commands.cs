using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using static System.Net.Mime.MediaTypeNames;
using DatabaseConnectionClass;
using CosmicBot;
//Custom Classes


namespace CosmicBotCommands
{
    class CosmicCommands
    {
        public string Help()
        {
            string help =
                "```1. Ping - \r\n\tThe Ping Command tests your Connection with the Bot, try it!" +
                "\r\n\r\n2. Echo - \r\n\tThe Echo Command splits the users message when the words are seperated by a \", \"" +
                "\r\n\r\n3. CreateNation - \r\n\t3a) adds the user to the database if they are not in it already" +
                "\r\n\t3b) adds the nation to the database linking it to the user" +
                "\r\n\r\n4. Nations - \r\n\t4a) with no user input, shows all the nations related to the author of the sent message" +
                "\r\n\t4b) if a @otheruser#4444 is used, shows all nations related to them instead " +
                "\r\n\r\n5. IsNation - \r\n\tdetermines if the requested nation exists or not (not case sensitive) " +
                "\r\n\r\n6. RenameNation - \r\n\tRenames the users nation if they exist as a user, and if their nation exists" +
                "```";
            return help;
        }
    }
}
