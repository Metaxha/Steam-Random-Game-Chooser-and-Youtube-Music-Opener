using System.Drawing;

namespace SteamGameTopMusicOpener.Services {
    public class ConsoleService {
        public void Initialize(string title) {
            Console.Title = title;
            Console.ForegroundColor = ConsoleColor.Cyan;
        }

        static Color ColorFromConsoleColor(ConsoleColor cc) => cc switch {
            ConsoleColor.Black => Color.FromArgb(0, 0, 0),
            ConsoleColor.DarkBlue => Color.FromArgb(0, 0, 128),
            ConsoleColor.DarkGreen => Color.FromArgb(0, 128, 0),
            ConsoleColor.DarkCyan => Color.FromArgb(0, 128, 128),
            ConsoleColor.DarkRed => Color.FromArgb(128, 0, 0),
            ConsoleColor.DarkMagenta => Color.FromArgb(128, 0, 128),
            ConsoleColor.DarkYellow => Color.FromArgb(128, 128, 0),
            ConsoleColor.Gray => Color.FromArgb(192, 192, 192),
            ConsoleColor.DarkGray => Color.FromArgb(128, 128, 128),
            ConsoleColor.Blue => Color.FromArgb(0, 0, 255),
            ConsoleColor.Green => Color.FromArgb(0, 255, 0),
            ConsoleColor.Cyan => Color.FromArgb(0, 255, 255),
            ConsoleColor.Red => Color.FromArgb(255, 0, 0),
            ConsoleColor.Magenta => Color.FromArgb(255, 0, 255),
            ConsoleColor.Yellow => Color.FromArgb(255, 255, 0),
            ConsoleColor.White => Color.FromArgb(255, 255, 255),
            _ => Color.White
        };
        public ConsoleColor ClosestConsoleColor(Color color) {
            var consoleColors = Enum.GetValues(typeof(ConsoleColor)).Cast<ConsoleColor>();
            ConsoleColor bestMatch = ConsoleColor.White;
            double leastDistance = double.MaxValue;

            foreach (var cc in consoleColors) {
                Color c = ColorFromConsoleColor(cc);
                double distance = Math.Pow(color.R - c.R, 2) + Math.Pow(color.G - c.G, 2) + Math.Pow(color.B - c.B, 2);

                if (distance < leastDistance) {
                    leastDistance = distance;
                    bestMatch = cc;
                }
            }

            return bestMatch;
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
        public void WriteSlow(string text, int delay = 25, ConsoleColor? color = null) {
            var originalColor = Console.ForegroundColor;

            if (color.HasValue) {
                if (IsDarkColor(color.Value))
                    Console.BackgroundColor = ConsoleColor.White;
                else
                    Console.BackgroundColor = ConsoleColor.Black;

                Console.ForegroundColor = color.Value;
            }

            foreach (var c in text) {
                Console.Write(c);

                // Özel durumlar için hız ayarı
                switch (c) {
                    case '\n':
                        Thread.Sleep(delay * 3); // Satır sonlarında daha uzun bekle
                        break;
                    case '.':
                    case '!':
                    case '?':
                        Thread.Sleep(delay * 5); // Noktalama işaretlerinde dramatik duraklama
                        break;
                    default:
                        Thread.Sleep(delay);
                        break;
                }
            }

            Console.ForegroundColor = originalColor;
        }

        public void WriteLineSlow(string text, int delay = 25, ConsoleColor? color = null) {
            WriteSlow(text + Environment.NewLine, delay, color);
        }

        public void ShowProgressBar(int durationSeconds = 3, int width = 50) {
            Console.CursorVisible = false;
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;

            for (int i = 0; i <= width; i++) {
                Console.Write("\r[");
                Console.Write(new string('=', i));
                if (i < width) Console.Write(">");
                Console.Write(new string(' ', width - i));
                Console.Write($"] {i * 2}%");
                Thread.Sleep(durationSeconds * 1000 / width);
            }

            Console.ForegroundColor = originalColor;
            Console.CursorVisible = true;
            Console.WriteLine();
        }
    }
}