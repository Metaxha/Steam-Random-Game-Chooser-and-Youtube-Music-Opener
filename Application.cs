using SteamGameTopMusicOpener.Services;
using System.Drawing;

namespace SteamGameTopMusicOpener {
    public class Application {
        private readonly SteamService _steamService;
        private readonly YoutubeService _youtubeService;
        private readonly ConsoleService _consoleService;

        public Application() {
            _consoleService = new ConsoleService();
            _steamService = new SteamService();
            _youtubeService = new YoutubeService(_consoleService);
        }

        bool IsDarkColor(ConsoleColor color) {
            switch (color) {
                case ConsoleColor.Black:
                case ConsoleColor.DarkBlue:
                case ConsoleColor.DarkGreen:
                case ConsoleColor.DarkCyan:
                case ConsoleColor.DarkRed:
                case ConsoleColor.DarkMagenta:
                case ConsoleColor.DarkGray:
                    return true;
                default:
                    return false;
            }
        }

        public async Task RunAsync() {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            _consoleService.Initialize("Metaxha Test");

            var games = _steamService.GetInstalledGames();
            var selectedGame = _steamService.SelectRandomGame(games);

            Color? dominantColor = await _steamService.GetDominantColorFromSteamApp(selectedGame.AppId);

            var consoleColor = ConsoleColor.Magenta;
            if (dominantColor != null) {
                consoleColor = _consoleService.ClosestConsoleColor(dominantColor.Value);
            }
            _consoleService.WriteSlow($"\n{selectedGame.Name}", color: consoleColor);

            await _youtubeService.OpenGameMusicFullscreen(selectedGame.Name);
        }
    }
}