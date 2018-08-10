using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ProtectorVenari
{
    /// <summary>
    /// ProtectorVenari discord bot
    /// </summary>
    class Program
    {
        #region Constants

        /// <summary>
        /// Name of the bot on discord
        /// </summary>
        private const string BotName = "Protector Venari";

        /// <summary>
        /// ID for the main boundless server
        /// </summary>
        private const ulong MainBoundlessServer = 119962974533320704;

        /// <summary>
        /// ID for a server used only for the emotes
        /// </summary>
        private const ulong SimoydsPrivateServerForEmotes = 231603109976211457;

        #endregion

        #region Initialization

        /// <summary>
        /// Main application entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        static void Main(string[] args)
        {
            new Thread(() => new Program().MainAsync().GetAwaiter().GetResult()).Start();
        }

        /// <summary>
        /// Main async thread entry point
        /// </summary>
        public async Task MainAsync()
        {
            // Configure the discord connection
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance,
            });

            // Subscribe to the events that we want
            _client.Log += Log;
            _client.Ready += Ready;
            _client.MessageReceived += MessageReceived;

            // Login to the discord server
            string token = "TOKEN IS PRIVATE"; // Remember to keep this private!
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        #endregion

        #region Private fields

        /// <summary>
        /// The client we are using to communicate with the discord server
        /// </summary>
        private DiscordSocketClient _client;

        /// <summary>
        /// Persistance file used to store notification configs
        /// </summary>
        private HunterInfoFile _hunterInfo = new HunterInfoFile();

        /// <summary>
        /// The main boundless server
        /// </summary>
        SocketGuild _boundlessServer;

        /// <summary>
        /// "nope" emote for reacting to messages when permission is denied
        /// </summary>
        private IEmote _nopeEmote;

        #endregion

        #region Discord Callbacks

        /// <summary>
        /// Callback for log messages from the discord client
        /// </summary>
        /// <param name="arg">The log message</param>
        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when the discord client is connected and ready
        /// </summary>
        private Task Ready()
        {
            // Ensure name is correct
            if (_client.CurrentUser.Username != BotName)
            {
                _client.CurrentUser.ModifyAsync(cur => cur.Username = BotName);
            }

            foreach (SocketGuild curGuild in _client.Guilds)
            {
                if (curGuild.CurrentUser.Nickname != BotName)
                {
                    curGuild.CurrentUser.ModifyAsync(cur => cur.Nickname = BotName);
                }
            }

            // Cache the boundless server object
            _boundlessServer = _client.GetGuild(MainBoundlessServer);

            // Cache the "nope" emote object
            SocketGuild emoteServer = _client.GetGuild(SimoydsPrivateServerForEmotes);
            _nopeEmote = emoteServer.Emotes.First(e => e.Name == "nope");

            // Load/Create the notification configuration
            _hunterInfo.GetAll();
            _hunterInfo.Save();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when a message is recieved either in a channel the bot is in, or in DM
        /// </summary>
        /// <param name="arg">The received message</param>
        private async Task MessageReceived(SocketMessage arg)
        {
            // These commands work in a server or in DM
            if (arg.Content == "!ping")
            {
                // Basic ping to check if bot is alive
                await arg.Channel.SendMessageAsync("pong!");
            }
            else if (arg.Content.StartsWith("!hunt ") || arg.Content.StartsWith("!test "))
            {
                // Command to send hunt notification out. Test command for testing code without pinging.
                bool isTest = arg.Content.StartsWith("!test ");

                // Get the user's roles on main boundless discord
                string[] authorRoles = _boundlessServer.GetUser(arg.Author.Id).Roles.Select(cur => cur.Name).ToArray();

                // Only certain roles are allowed to perform the ping
                if (authorRoles.Contains("Oortian") ||
                    authorRoles.Contains("Oortling"))
                {
                    if (arg.Content.StartsWith("!hunt config"))
                    {
                        // Prevent accidental mistakes for the config command
                        await arg.Channel.SendMessageAsync("Do you mean `!config hunt`?");
                    }
                    else
                    {
                        // Loop through all configured servers
                        foreach (HunterInfo curInfo in _hunterInfo.GetAll())
                        {
                            // Check if bot was kicked from the server
                            if (!_client.Guilds.Select(cur => cur.Id).Contains(curInfo.Guild))
                            {
                                _hunterInfo.Remove(curInfo.Guild);
                                continue;
                            }

                            SocketRole role = null;

                            if (curInfo.Role != 0)
                            {
                                // Get the role
                                try
                                {
                                    role = _client.GetGuild(curInfo.Guild).Roles.FirstOrDefault(cur => cur.Id == curInfo.Role);
                                }
                                catch { }
                            }

                            if (role != null)
                            {
                                // Mark the hunter hole as pingable
                                try
                                {
                                    await role.ModifyAsync((a) =>
                                    {
                                        a.Mentionable = true;
                                    });
                                }
                                catch { }
                            }

                            // Do the notification
                            try
                            {
                                var channel = _client.GetChannel(curInfo.Channel) as ISocketMessageChannel;

                                string pingString;

                                if (role != null)
                                {
                                    if (isTest)
                                    {
                                        pingString = $"**{role.Name}** `From {arg.Author.Username}({arg.Author.Id})` Test";
                                    }
                                    else
                                    {
                                        pingString = $"<@&{role.Id}> `From {arg.Author.Username}({arg.Author.Id})` Hunt";
                                    }
                                }
                                else
                                {
                                    if (isTest)
                                    {
                                        pingString = $"`From {arg.Author.Username}({arg.Author.Id})` Test";
                                    }
                                    else
                                    {
                                        pingString = $"`From {arg.Author.Username}({arg.Author.Id})` Hunt";
                                    }
                                }

                                await channel.SendMessageAsync($"{pingString} {arg.Content.Substring("!hunt ".Length)}");
                            }
                            catch { }

                            if (role != null)
                            {
                                // Mark the hunter hole as not pingable
                                try
                                {
                                    await role.ModifyAsync((a) =>
                                    {
                                        a.Mentionable = false;
                                    });
                                }
                                catch { }
                            }
                        }
                    }
                }
                else
                {
                    // Permission denied
                    await((SocketUserMessage)arg).AddReactionAsync(_nopeEmote);
                }
            }
            else if (!(arg.Channel is SocketGuildChannel))
            {
                // Commands below require a server (DM not supported)
                return;
            }

            // The following commands only work on a server
            SocketGuild curGuild = ((SocketGuildChannel)arg.Channel).Guild;

            if (arg.Content.StartsWith("!listroles"))
            {
                // Lists the role IDs on the server (because there's no easy way to get this from the client without pinging the role)
                await arg.Author.SendMessageAsync(string.Join("\r\n", ((SocketGuildChannel)arg.Channel).Guild.Roles.Select(cur => $"{cur.Name} - {cur.Id}").ToArray()));
            }
            else if (arg.Content.StartsWith("!config hunt"))
            {
                // Command to update what role is pinged and what channel the notification goes to on a server

                // Only allow administrators on the server update the config
                if (((SocketGuildUser)arg.Author).Roles.Any(cur => cur.Permissions.Administrator))
                {
                    // Get the existing config, if one exists
                    HunterInfo info = _hunterInfo.GetGuild(curGuild.Id);

                    // If there's at least a space after the command, then the user is trying to update the config
                    if (arg.Content.StartsWith("!config hunt "))
                    {
                        string target = arg.Content.Substring("!config hunt ".Length);

                        // Anything following the command will be the role ping or role ID, so parse it out as a ulong
                        Regex pingMatch = new Regex("<@&([0-9]+)>");
                        Regex idMatch = new Regex("([0-9]+)");

                        ulong id = 0;
                        bool noRole = false;

                        if (pingMatch.IsMatch(target))
                        {
                            id = Convert.ToUInt64(target.Substring(3, target.Length - 4));
                        }
                        else if (idMatch.IsMatch(target))
                        {
                            id = Convert.ToUInt64(target);
                        }
                        else if (target == "norole")
                        {
                            noRole = true;
                        }

                        // Get the role object for the specified ID
                        SocketRole targetRole = null;

                        if (id != 0)
                        {
                            targetRole = curGuild.GetRole(id);
                        }

                        // If the target role is valid, then update the saved info for this server
                        // Use the channel that the config command was sent in for the notifications
                        if (targetRole != null || noRole)
                        {
                            info = new HunterInfo
                            {
                                Guild = curGuild.Id,
                                Channel = arg.Channel.Id,
                                Role = noRole ? 0 : targetRole.Id,
                            };

                            _hunterInfo.UpdateInfo(info);
                        }
                    }

                    // Regardless if the user updated the config or not, output the current config status
                    if (info == null)
                    {
                        await arg.Channel.SendMessageAsync("Hunt notifications are not enabled on this server.");
                    }
                    else
                    {
                        var role = curGuild.Roles.FirstOrDefault(cur => cur.Id == info.Role);
                        var channel = curGuild.Channels.FirstOrDefault(cur => cur.Id == info.Channel);

                        await arg.Channel.SendMessageAsync($"Hunt notifications will {(role == null ? "be in" : $"ping `{role.Name}` in")} channel `#{(channel == null ? info.Channel.ToString() : channel.Name)}` on this server.");
                    }
                }
                else
                {
                    // Permission denied
                    await ((SocketUserMessage)arg).AddReactionAsync(_nopeEmote);
                }
            }
            else if (arg.Content == "!hunter")
            {
                // Command for anyone to give or remove the hunter role from themself

                // Get the hunter role for the current server
                HunterInfo info = _hunterInfo.GetGuild(curGuild.Id);
                SocketRole role = curGuild.GetRole(info.Role);

                if (role != null)
                {
                    if (((SocketGuildUser)arg.Author).Roles.Contains(role))
                    {
                        // Remove the role if the user already has it
                        try
                        {
                            await((SocketGuildUser)arg.Author).RemoveRoleAsync(role).ContinueWith(async (task)
                               => await arg.Channel.SendMessageAsync("You are no longer a hunter!"), TaskContinuationOptions.NotOnFaulted);
                        }
                        catch { }
                    }
                    else
                    {
                        // Add the role if the user doesn't have it
                        try
                        {
                            await((SocketGuildUser)arg.Author).AddRoleAsync(role).ContinueWith(async (task)
                               => await arg.Channel.SendMessageAsync("You are now a hunter!"), TaskContinuationOptions.NotOnFaulted);
                        }
                        catch { }
                    }
                }
            }
        }

        #endregion
    }
}
