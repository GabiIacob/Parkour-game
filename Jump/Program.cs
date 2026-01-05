using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;

namespace Jump
{
    public static class Program
    {
        public static void Main()
        {
            var nws = new NativeWindowSettings()
            {
                Title = "Jump Game",
                WindowState = WindowState.Fullscreen,
                Flags = ContextFlags.ForwardCompatible,
            };

            using (Game game = new Game(GameWindowSettings.Default, nws))
            {
                game.Run();
            }
        }
    }
}