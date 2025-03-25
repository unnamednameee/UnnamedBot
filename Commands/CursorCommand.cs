using System.ComponentModel;
using System.Runtime.InteropServices;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.SlashCommands.Metadata;
using DSharpPlus.Entities;

namespace UnnamedBot.Commands;

[Command("cursor"), Description("Interact with mouse cursor of unnamedbot's creator")]
[InteractionInstallType(DiscordApplicationIntegrationType.UserInstall)]
public class CursorCommand
{
    private const int MaxScreenX = 1920 - 1;
    private const int MaxScreenY = 1080 - 1;

    [Command("setpos"), Description("Move mouse cursor of unnamedbot's creator")]
    public class SetPosition
    {
        [Command("coords"), Description("Move mouse cursor of unnamedbot's creator to specified coordinates")]
        public static async ValueTask CoordinatesAsync(CommandContext context, [MinMaxValue(0, MaxScreenX)] int x, [MinMaxValue(0, MaxScreenY)] int y)
        {
            await MoveCursor(context, x, y);
        }

        [Command("random"), Description("Move mouse cursor of unnamedbot's creator to random coordinates")]
        public static async ValueTask RandomCoordinatesAsync(CommandContext context)
        {
            var random = new Random();
            int x = random.Next(MaxScreenX);
            int y = random.Next(MaxScreenY);
            await MoveCursor(context, x, y);
        }

        private static async ValueTask MoveCursor(CommandContext context, int x, int y)
        {
            SetCursorPos(x, y);
            await context.RespondAsync($"Mouse cursor of unnamedbot's creator has been moved to X: `{x}`, Y: `{y}`");
        }

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);
    }
}
