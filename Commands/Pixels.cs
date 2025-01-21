using System.ComponentModel;
using System.Globalization;
using System.Text;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.MessageCommands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.Metadata;
using DSharpPlus.Entities;

namespace UnnamedBot.Commands;

public class Pixels
{
    [Command("pixels"), Description("Count pixels in an attachment")]
    [InteractionInstallType(DiscordApplicationIntegrationType.UserInstall)]
    public static async ValueTask ExecuteSlashCommandAsync(CommandContext context, [Description("Attachment to count pixels in")] DiscordAttachment attachment)
    {
        await context.DeferResponseAsync();
        await context.RespondAsync(GetResponse(attachment));
    }

    [Command("pixels-m"), DisplayName("Count pixels"), Description("Count pixels in attachments")]
    [SlashCommandTypes(DiscordApplicationCommandType.MessageContextMenu)]
    [InteractionInstallType(DiscordApplicationIntegrationType.UserInstall)]
    public static async ValueTask ExecuteMessageCommandAsync(MessageCommandContext context, DiscordMessage message)
    {
        await context.DeferResponseAsync();

        var attachments = message.Attachments;
        if (attachments.Count == 0)
        {
            await context.RespondAsync("This message doesn't have attachments.");
            return;
        }
        else if (attachments.Count == 1)
        {
            var attachment = attachments[0];
            await context.RespondAsync(GetResponse(attachment));
            return;
        }

        var responseBuilder = new StringBuilder();
        responseBuilder.AppendLine($"This message has {attachments.Count} attachments:");
        var totalPixelsCount = 0;
        for (int i = 0; i < attachments.Count; i++)
        {
            responseBuilder.AppendLine($"{i + 1}. {GetResponse(attachments[i])}");
            totalPixelsCount += GetPixelsCount(attachments[i]);
        }
        responseBuilder.AppendLine($"Total pixels: {Format(totalPixelsCount)}.");
        await context.RespondAsync(responseBuilder.ToString());
    }

    private static int GetPixelsCount(DiscordAttachment attachment)
        => attachment.Width is not null && attachment.Height is not null
            ? (int)attachment.Width * (int)attachment.Height
            : 0;

    private static string Format(int number) => number.ToString("N0", CultureInfo.GetCultureInfo("en-US"));

    private static string Format(int? number) => ((int)number!).ToString("N0", CultureInfo.GetCultureInfo("en-US"));

    private static string GetResponse(DiscordAttachment attachment)
    {
        var invalidFileResponse = "This attachment is not supported.";
        var wtfResponse = "Couldn't count pixels; something went wrong.";

        try
        {
            string? mediaType = attachment.MediaType; // "{type}/{extension}"
            if (mediaType == null)
                return invalidFileResponse;

            if (GetPixelsCount(attachment) == 0)
                return wtfResponse;

            var mediaTypeAndExtension = mediaType.Split('/'); // 0 - type, 1 - extension
            return mediaTypeAndExtension[0] switch // checking media type without extension
            {
                "image" => $"{(mediaTypeAndExtension[1] != "gif" ? "This image has" : "A frame in this GIF has")} {FormatPixelsCountInfo(attachment)}",
                "video" => $"A frame in this video has {FormatPixelsCountInfo(attachment)}",
                _ => invalidFileResponse
            };
        }
        catch
        {
            return wtfResponse;
        }
    }

    private static string FormatPixelsCountInfo(DiscordAttachment attachment)
        => $"{Format(GetPixelsCount(attachment))} ({Format(attachment.Width)}×{Format(attachment.Height)}) pixel{(GetPixelsCount(attachment) == 1 ? "" : "s")}.";
}
