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
        await context.RespondAsync(GetPixelsInAttachmentInfo(attachment));
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
            var response = "This message doesn't have attachments.";
            await context.RespondAsync(response);
            return;
        }
        else if (attachments.Count == 1)
        {
            var attachment = attachments[0];
            var response = GetPixelsInAttachmentInfo(attachment);
            await context.RespondAsync(response);
            return;
        }

        // if there are multiple attachments, count all of their pixels
        var responseBuilder = new StringBuilder();
        responseBuilder.AppendLine($"This message has {attachments.Count} attachments:");
        var totalPixelsCount = 0;
        for (int i = 0; i < attachments.Count; i++)
        {
            var attachment = attachments[i];
            var pixelsInfo = GetPixelsInAttachmentInfo(attachment);
            responseBuilder.AppendLine($"{i + 1}. {pixelsInfo}");

            var pixelsCount = GetPixelsCount(attachment);
            totalPixelsCount += pixelsCount;
        }
        var formattedTotalPixels = FormatWithCommas(totalPixelsCount);
        responseBuilder.AppendLine($"Total pixels: {formattedTotalPixels}.");
        await context.RespondAsync(responseBuilder.ToString());
    }

    private static int GetPixelsCount(DiscordAttachment attachment)
        => attachment.Width is not null && attachment.Height is not null
            ? (int)attachment.Width * (int)attachment.Height
            : 0;

    // adds commas to big numbers (12345678 -> 12,345,678)
    private static string FormatWithCommas(int number)
        => number.ToString("N0", CultureInfo.GetCultureInfo("en-US"));

    // same thing but for nullable integers
    private static string FormatWithCommas(int? number)
        => ((int)number!).ToString("N0", CultureInfo.GetCultureInfo("en-US"));

    private static string GetPixelsInAttachmentInfo(DiscordAttachment attachment)
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

            var mediaTypeAndExtension = mediaType.Split('/'); // [0] - type, [1] - extension
            return mediaTypeAndExtension[0] switch // checking media type without extension
            {
                "image" => $"{(mediaTypeAndExtension[1] != "gif" ? "This image has" : "A frame in this GIF has")} {FormatPixelsCount(attachment)}",
                "video" => $"A frame in this video has {FormatPixelsCount(attachment)}.",
                _ => invalidFileResponse
            };
        }
        catch
        {
            return wtfResponse;
        }
    }

    private static string FormatPixelsCount(DiscordAttachment attachment)
    {
        int attachmentPixelsCount = GetPixelsCount(attachment);
        string attachmentPixels = FormatWithCommas(attachmentPixelsCount);
        string width = FormatWithCommas(attachment.Width);
        string height = FormatWithCommas(attachment.Height);
        string s = attachmentPixelsCount == 1 ? "" : "s";
        return $"{attachmentPixels} ({width}×{height}) pixel{s}";
    }
}
