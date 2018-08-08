# ProtectorVenari
This is a bot used by the discord community accross multiple servers to notify people of a community hunt. To invite this bot to your server, use this link: https://discordapp.com/api/oauth2/authorize?client_id=356915648678592513&permissions=0&scope=bot

## Commands
The following are commands which are available for the bot

### !ping
This command can be run on a server or in DM by anyone. The bot will simply respond with "pong!"

### !listroles
This command must be run on a server. This command can be run by anyone. This command will DM the caller a list of roles and their IDs from the server that the command was run on. This is to provide ease of use to obtain role IDs without having to ping a role. There are other cumbersome ways to get the role ID so this is not a security issue as the information is already public.

### !config hunt <role>
This command is only allowed by users with the "Administrator" permission on the server the command is run on. This command must be run on a server. This command will enable hunt notifications in the channel that the command was run in. <role> can be a mention (ping) of a role "<@&id>", or simply the role id by itself. When a valid !hunt command is later issued, the bot will try to set the role as pingable, ping the role with the message in the !hunt command, then set the role back to unpingable. If the bot doesn't have manage role permissions for role, then this will fail silently, but the message will still be sent to the configured channel.

### !hunter
This command must be run on a server. This command can be run by anyone. If the user does not already have the configured role, the bot will attempt to grant the configured role to the user. If the user already has the role, the bot will remove it from the user. If the bot is successfull, a message will be displayed indicating such in the channel the command was run in. If the bot fails to adjust the role, it will remain silent.

### !hunt <message>
This command can be run on a server or in DM by users with the "Oortian" and/or "Oortling" roles. This command must have a message to work. When this command is sent, all servers with notifications configured will be notified with the message specified by this command. See "!config hunt" above for notification configuration, and how roles are pinged on servers.
