using System.ComponentModel;
using System.Runtime.InteropServices;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;

namespace UnnamedBot.Commands;

[Command("cursor"), Description("Interact with mouse cursor of the owner of this server")]
public class Cursor
{
    private const int MaxScreenX = 1920 - 1;
    private const int MaxScreenY = 1080 - 1;

    [Command("setpos"), Description("Move mouse cursor of the owner of this server")]
    public class SetPosition
    {
        [Command("coords"), Description("Move mouse cursor of the owner of this server to specified coordinates")]
        public static async ValueTask CoordinatesAsync(CommandContext context, [MinMaxValue(0, MaxScreenX)] int x, [MinMaxValue(0, MaxScreenY)] int y)
        {
            await context.DeferResponseAsync();
            await Do(context, x, y);
        }

        [Command("random"), Description("Move mouse cursor of the owner of this server to random coordinates")]
        public static async ValueTask RandomCoordinatesAsync(CommandContext context)
        {
            await context.DeferResponseAsync();

            var random = new Random();
            int x = random.Next(MaxScreenX);
            int y = random.Next(MaxScreenY);
            await Do(context, x, y);
        }

        private static async ValueTask Do(CommandContext context, int x, int y)
        {
            SetCursorPos(x, y);
            await context.RespondAsync($"Mouse cursor of the owner of this server has been moved to X: `{x}`, Y: `{y}`");
        }

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);
    }
}
