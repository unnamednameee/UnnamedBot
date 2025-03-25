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
    private static readonly Random Random = new();

    private static async Task Main(string[] args)
    {
        if (OperatingSystem.IsWindows())
        {
            Console.SetWindowSize(200, 50);
        }

        string environmentVariableWithToken = "UNNAMEDBOT";
#if DEBUG
        environmentVariableWithToken += "_DEBUG";
#endif
        var token = Environment.GetEnvironmentVariable(environmentVariableWithToken, EnvironmentVariableTarget.User);
        if (token is null)
        {
            Console.WriteLine($"Couldn't find bot's token. Provide it in the \"{environmentVariableWithToken}\" environment variable, then restart the program.");
            Environment.Exit(0);
        }

        var builder = DiscordClientBuilder.CreateDefault(token, DiscordIntents.AllUnprivileged);
        builder.UseCommands((IServiceProvider serviceProvider, CommandsExtension extension) =>
        {
            extension.AddCommands([typeof(UnnamedBotCommand), typeof(PixelsCommand), typeof(CursorCommand)]);

            extension.AddProcessor(new TextCommandProcessor(new TextCommandConfiguration()
            {
                PrefixResolver = new DefaultPrefixResolver(false, ">").ResolvePrefixAsync
            }));
            extension.AddProcessor(new MessageCommandProcessor());

            extension.CommandErrored += CommandsExtension_CommandErrored;
            extension.CommandExecuted += CommandsExtension_CommandExecuted;
        }, new CommandsConfiguration()
        {
            UseDefaultCommandErrorHandler = false
        });

        await ConnectBotAsync(builder);
        await Task.Delay(-1);
    }

    private static async Task ConnectBotAsync(DiscordClientBuilder builder, int connectAttempt = 0)
    {
        Client = builder.Build();

        try
        {
            connectAttempt++;
            await Client.ConnectAsync();
            UptimeStopwatch.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Console.WriteLine($"Failed to connect. ({connectAttempt})");
            if (connectAttempt <= 2)
            {
                Console.WriteLine("Retrying in 10 seconds...");
                await Task.Delay(10000);
                await ConnectBotAsync(builder, connectAttempt);
            }
        }
    }

    private static async Task CommandsExtension_CommandErrored(CommandsExtension sender, CommandErroredEventArgs e)
    {
        var errorResponsesList = File.ReadAllLines("error-responses.txt");
        var rareErrorResponse = Formatter.ToSmallHeader("✨ You've found the rarest error message! (1% chance)");

        var responseBuilder = new StringBuilder();
        responseBuilder.AppendLine("Something went wrong! Please report this issue to unnamedbot's creator (@unnamedname).");
        responseBuilder.AppendLine(Formatter.BlockCode(e.Exception.ToString()));
        if (Random.Next(100) == 0)
        {
            responseBuilder.AppendLine(rareErrorResponse);
        }
        else
        {
            var errorResponse = errorResponsesList[Random.Next(errorResponsesList.Length)];
            responseBuilder.AppendLine(errorResponse);
        }

        await e.Context.RespondAsync(responseBuilder.ToString());
        Console.WriteLine(e.Exception.ToString());
    }

    private static async Task CommandsExtension_CommandExecuted(CommandsExtension sender, CommandExecutedEventArgs e)
    {
        await LogCommandExecution(e);
    }

    private static async Task LogCommandExecution(CommandExecutedEventArgs e)
    {
        var response = await e.Context.GetResponseAsync();
        if (response is null) return;

        var logBuilder = new StringBuilder();
        logBuilder.Append($"[{response.Timestamp}] ");
        var channel = response.Channel;
        if (channel is not null)
        {
            if (channel.Guild is not null)
            {
                logBuilder.Append($"{channel.Guild.Name} ({channel.Guild.Id}): ");
            }
            logBuilder.Append($"{channel.Name} ({channel.Id}): ");
        }
        logBuilder.Append($"{e.Context.User.Username} ({e.Context.User.Id}) ");
        logBuilder.Append($"=> /{e.Context.Command.FullName}");
        foreach (var p in e.Context.Command.Parameters)
        {
            logBuilder.Append($" {p.Name}");
        }
        Console.WriteLine(logBuilder.ToString());
    }
}
