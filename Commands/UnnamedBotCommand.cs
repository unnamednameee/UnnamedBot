using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands.Metadata;
using DSharpPlus.Entities;

namespace UnnamedBot.Commands;

public class UnnamedBotCommand
{
    [Command("unnamedbot"), Description("Get information about unnamedbot")]
    [InteractionInstallType(DiscordApplicationIntegrationType.UserInstall)]
    public static async ValueTask ExecuteAsync(CommandContext context)
    {
        await context.DeferResponseAsync();

        var dspRepositoryLink = "[DSharpPlus](https://github.com/DSharpPlus/DSharpPlus)";
        var embed = new DiscordEmbedBuilder()
            .WithAuthor(name: context.User.Username,
                        iconUrl: context.User.AvatarUrl)
            .WithTitle(context.Client.CurrentUser.Username)
            .WithDescription($"shitty bot (being) developed with C# and {dspRepositoryLink} for all kinds of toilets")
            .WithThumbnail(context.Client.CurrentUser.AvatarUrl)
            .AddField("Latency", $"{GetLatencyInMilliseconds(context)} ms", true)
            .AddField("Version", Program.Version, true)
            .AddField("Uptime", GetFormattedUptime(), true)
            .WithTimestamp(DateTimeOffset.UtcNow);
        await context.RespondAsync(embed.Build());
    }

    private static int GetLatencyInMilliseconds(CommandContext context)
    {
        // context.Guild is null when user executes the command in bot's DM.
        // so when they do, the latency is taken from "unnamedbot's home" server.
        // (it's not like latency is the same in all guilds)
        // until i figure out how to fix the DM issue properly, this is the solution.
        TimeSpan latency = context.Client.GetConnectionLatency(context.Guild is not null ? context.Guild.Id : Program.HomeGuildId);
        return latency.Milliseconds
             + latency.Seconds * 1000
             + latency.Minutes * 60 * 1000;
    }

    private static string GetFormattedUptime()
    {
        TimeSpan uptime = Program.UptimeStopwatch.Elapsed;
        return uptime.ToString(uptime.Days >= 1 ? @"d\:hh\:mm\:ss\.fff" : @"hh\:mm\:ss\.fff");
    }
}
