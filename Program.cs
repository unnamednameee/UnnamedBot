// https://discord.com/oauth2/authorize?client_id=1297253236620787844&permissions=564444602166470&integration_type=0&scope=bot
using System.Diagnostics;
using System.Text;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Processors.MessageCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using UnnamedBot.Commands;

namespace UnnamedBot;

internal class Program
{
    internal const ulong HomeGuildId = 1297255582159601728;
    internal const ulong TestGuildId = 1051610334965796914;
    internal const string Version = "0.2.0";
    internal static Stopwatch UptimeStopwatch { get; } = new();
    private static DiscordClient? Client { get; set; }

    private static async Task Main(string[] args)
    {
        if (OperatingSystem.IsWindows())
        {
            Console.SetWindowSize(200, 50);
        }

        var token = Environment.GetEnvironmentVariable("UNNAMEDBOT", EnvironmentVariableTarget.User);
        if (token is null)
        {
            Console.WriteLine("you're stupid");
            Environment.Exit(0);
        }

        var builder = DiscordClientBuilder.CreateDefault(token, DiscordIntents.AllUnprivileged);
        builder.UseCommands((IServiceProvider serviceProvider, CommandsExtension extension) =>
        {
            extension.AddCommands([typeof(UnnamedBotInfo), typeof(Pixels)]);
            extension.AddCommands([typeof(Cursor)], HomeGuildId);

            extension.AddProcessor(new TextCommandProcessor(new TextCommandConfiguration()
            {
                PrefixResolver = new DefaultPrefixResolver(false, ">").ResolvePrefixAsync
            }));
            extension.AddProcessor(new MessageCommandProcessor());

            extension.CommandErrored += CommandsExtension_CommandErrored;
            extension.CommandExecuted += CommandsExtension_CommandExecuted;
        }, new CommandsConfiguration()
        {
            UseDefaultCommandErrorHandler = false,
        });

        Client = builder.Build();
        await ConnectBotAsync();
        await Task.Delay(-1);
    }

    private static async Task ConnectBotAsync()
    {
        if (Client is null)
        {
            Console.WriteLine("DiscordClient is not built. RIP");
            return;
        }

        try
        {
            await Client.ConnectAsync();
            UptimeStopwatch.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Console.WriteLine("Failed to connect. Retrying in 10 seconds...");
            await Task.Delay(10000);
            await ConnectBotAsync();
        }
    }

    private static async Task CommandsExtension_CommandErrored(CommandsExtension sender, CommandErroredEventArgs e)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("An error occurred while executing the command: ");
        stringBuilder.Append(e.Exception.GetType().Name);
        stringBuilder.Append(", ");
        stringBuilder.Append(Formatter.InlineCode(Formatter.Sanitize(e.Exception.Message)));

        await e.Context.RespondAsync(stringBuilder.ToString());
        Console.WriteLine(stringBuilder);
    }

    private static async Task CommandsExtension_CommandExecuted(CommandsExtension sender, CommandExecutedEventArgs e)
    {
        // unfortunately idk how to log shit properly yet so i'll use cw for now
        var responseMsg = await e.Context.GetResponseAsync();
        if (responseMsg is null)
            return;

        //var logBuilder = new StringBuilder();
        //logBuilder.Append($"[{responseMsg.Timestamp}] ");
        //logBuilder.Append($"{responseMsg.Channel!.Guild.Name} ({responseMsg.Channel.GuildId}): ");
        //logBuilder.Append($"{responseMsg.Channel.Name} ({responseMsg.ChannelId}): ");
        //logBuilder.Append($"{e.Context.User.Username} ({e.Context.User.Id}) ");
        //logBuilder.Append($"=> /{e.Context.Command.FullName}");
        //Console.WriteLine(logBuilder.ToString());
        Console.WriteLine(responseMsg.JumpLink);
    }
}
